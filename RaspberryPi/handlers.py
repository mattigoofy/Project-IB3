from UART import *
from time import sleep

#
# Verwerk login informatie
#
def handle_login(payload):
    try:
        payload = decrypt(payload)
        data = json.loads(payload)
        username = data.get("Username")
        password = data.get("Password")
        # password = decrypt(password)

        connection = connect_to_db()
        if connection:
            cursor = connection.cursor(dictionary=True)
            cursor.execute("SELECT * FROM users WHERE username = %s AND password = %s", (username, password))
            user = cursor.fetchone()
            if user:
                user_id = user.get("id")
                print("Login geslaagd!")
                # Controleer of user_id aanwezig is en stuur alleen dan het volledige object
                if user_id is not None:
                    response_payload = encrypt(json.dumps({"success": True, "userId": user_id, "username": username}))
                    client.publish("raspberrypi/login/response", response_payload)
                else:
                    response_payload = encrypt(json.dumps({"success": False, "error": "User ID not found"}))
                    client.publish("raspberrypi/login/response", response_payload)
            else:
                print("Login mislukt!")
                response_payload = encrypt(json.dumps({"success": False}))
                client.publish("raspberrypi/login/response", response_payload)
            connection.close()
    except json.JSONDecodeError:
        print("Fout bij het parsen van login payload.")
        response_payload = encrypt(json.dumps({"success": False}))
        client.publish("raspberrypi/login/response", response_payload)



#
# Verwerk registratie informatie
#
def handle_register(payload):
    try:
        payload = decrypt(payload)
        data = json.loads(payload)
        username = data.get("Username")
        password = data.get("Password")
        # password = decrypt(password)
        email = data.get("Email")

        if not is_valid_email(email):
            print("Registratie mislukt! Ongeldig e-mailadres.")
            client.publish("raspberrypi/register/response", "Invalid email address")
            return

        connection = connect_to_db()
        if connection:
            cursor = connection.cursor()
            cursor.execute("SELECT * FROM users WHERE username = %s", (username,))
            existing_user = cursor.fetchone()

            if existing_user:
                print("Gebruikersnaam al in gebruik.")
                client.publish("raspberrypi/register/response", "Username already exists")
                connection.close()
                return

            cursor.execute("INSERT INTO users (username, password, email) VALUES (%s, %s, %s)", (username, password, email))
            connection.commit()
            print("Registratie geslaagd!")
            response_payload = encrypt("Registration successful")
            client.publish("raspberrypi/register/response", response_payload)
            connection.close()
    except json.JSONDecodeError:
        print("Fout bij het parsen van registratie payload.")
        response_payload = encrypt("Invalid payload format")
        client.publish("raspberrypi/register/response", response_payload)




#
# Verwerk het uitvoeren van trajecten
#
def handle_execute_command(payload):
    try:
        data = json.loads(payload)
        command = data.Command
        user_id = data.userId

        execute_1_command(command)

        response_payload = "true"
        client.publish("raspberrypi/execute_command/response", response_payload)

    except json.JSONDecodeError:
        print("Fout bij het parsen van execute_traject payload.")
        client.publish("raspberrypi/execute_traject/response", "false")




#
# Verwerk het uitvoeren van trajecten
#
def handle_execute_traject(payload):
    try:
        data = json.loads(payload)
        traject_id = data.trajectId
        user_id = data.userId

        connection = connect_to_db()
        if connection:
            cursor = connection.cursor(dictionary=True)
            cursor.execute("SELECT * FROM trajects WHERE user_id = %s", (user_id,))
            trajects = cursor.fetchall()

            for traject in trajects:
                if traject_id == traject["id"]:
                    cursor.execute("SELECT * FROM commands WHERE traject_id = %s", (traject_id,))
                    cmds_to_exe = cursor.fetchall()
                    break

            print(f"Commands gevonden: {cmds_to_exe}")
            connection.close()
        else:
            print("something went wrong")

        if cmds_to_exe != None:
            for cmd in cmds_to_exe:
                execute_1_command(cmd)


        response_payload = "true"
        client.publish("raspberrypi/execute_traject/response", response_payload)

    except json.JSONDecodeError:
        print("Fout bij het parsen van execute_traject payload.")
        response_payload = "false"
        client.publish("raspberrypi/execute_traject/response", response_payload)




#
# Verwijder trajecten
#
def handle_delete_traject(payload):
    try:
        data = json.loads(payload)
        traject_id = data.get("TrajectId")
        connection = connect_to_db()

        if connection:
            cursor = connection.cursor()
            cursor.execute("DELETE FROM commands WHERE traject_id = %s", (traject_id,))
            cursor.execute("DELETE FROM trajects WHERE id = %s", (traject_id,))
            connection.commit()
            connection.close()
            print(f"Traject {traject_id} verwijderd.")
            client.publish("raspberrypi/delete_traject/response", "true")
        else:
            client.publish("raspberrypi/delete_traject/response", "false")
    except json.JSONDecodeError:
        print("Fout bij het parsen van delete_traject payload.")
        client.publish("raspberrypi/delete_traject/response", "false")




#
# Laad trajecten voor de gebruiker
#
def handle_load_trajects(payload):
    try:
        data = json.loads(payload)
        print(f"Ontvangen load trajects payload: {data}")  # Debug info
        user_id = data.get("UserId")

        connection = connect_to_db()
        if connection:
            cursor = connection.cursor(dictionary=True)
            cursor.execute("SELECT * FROM trajects WHERE user_id = %s", (user_id,))
            trajects = cursor.fetchall()

            for traject in trajects:
                traject_id = traject["id"]
                cursor.execute("SELECT * FROM commands WHERE traject_id = %s", (traject_id,))
                traject["commands"] = cursor.fetchall()

                # Converteer datetime objecten naar strings
                traject['created_at'] = traject['created_at'].isoformat()  # Converteer created_at
                for command in traject["commands"]:
                    command['created_at'] = command['created_at'].isoformat()  # Converteer created_at van commands

            print(f"Trajecten gevonden: {trajects}")  # Debug info
            client.publish("raspberrypi/load_trajects/response", json.dumps(trajects, default=datetime_converter))
            connection.close()
        else:
            client.publish("raspberrypi/load_trajects/response", "[]")
    except json.JSONDecodeError:
        print("Fout bij het parsen van load_trajects payload.")
        client.publish("raspberrypi/load_trajects/response", "[]")




#
# Verwerk opslaan van trajecten
#
def handle_save_traject(payload):
    try:
        data = json.loads(payload)
        print(f"Ontvangen traject payload: {data}")  # Debug info

        user_id = data.get("UserId")
        traject = data.get("Traject")
        name = traject.get("Name")
        commands = traject.get("Commands")

        connection = connect_to_db()
        if connection:
            cursor = connection.cursor()
            cursor.execute("INSERT INTO trajects (name, user_id) VALUES (%s, %s)", (name, user_id))
            traject_id = cursor.lastrowid

            for command in commands:
                action = command.get("Action")
                duration = command.get("Duration")
                cursor.execute("INSERT INTO commands (action, duration, traject_id) VALUES (%s, %s, %s)", (action, duration, traject_id))
            
            connection.commit()
            connection.close()
            print(f"Traject '{name}' opgeslagen met ID: {traject_id}")
            client.publish("raspberrypi/save_traject/response", "true")
        else:
            client.publish("raspberrypi/save_traject/response", "false")
    except json.JSONDecodeError:
        print("Fout bij het parsen van save_traject payload.")
        client.publish("raspberrypi/save_traject/response", "false")




#
# 1 commando uitvoeren
#
def execute_1_command(cmd):
    print(f"i'm sending {cmd} over UART B-)")
    action = cmd.Action
    duration = cmd.Duration
    name: str = cmd.Name

    bits: str

    if   action == "Vooruit":
        bits = "000"
    elif action == "Achteruit":
        bits = "001"
    elif action == "Links":
        bits = "010"
    elif action == "Rechts":
        bits = "011"
    elif action == "Links_draaien":
        bits = "100"
    elif action == "Rechts_draaien":
        bits = "101"
    elif action == "Measurement":
        bits = "110"
    elif action == "Idle":
        bits = "111"

    bits += "10000"     # half max speed

    UART_send(bits)
    sleep(duration)     # kwn als sleep goed idee is

