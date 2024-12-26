import re  # Voor e-mailvalidatie
from UART import *
from testCrypt import *
import json
from time import sleep
from mariaDB import *
# import datetime as dt
from datetime import datetime as dt

#
# Verwerk login informatie
#
def handle_login(payload: json) -> json:
    try:
        payload = decrypt(payload)
        data = json.loads(payload)
        username = data.get("Username")
        password = data.get("Password")

        connection = connect_to_db()
        if connection:
            cursor = connection.cursor(dictionary=True)
            cursor.execute("SELECT * FROM users WHERE username = %s AND password = %s", (username, password))
            user = cursor.fetchone()
            if user:
                user_id = user.get("id")
                print("\tLogin geslaagd!")
                # Controleer of user_id aanwezig is en stuur alleen dan het volledige object
                if user_id is not None:
                    response_payload = encrypt(json.dumps({"success": True, "userId": user_id, "username": username}))
                else:
                    response_payload = encrypt(json.dumps({"success": False, "error": "User ID not found"}))
            else:
                print("\tLogin mislukt!")
                response_payload = encrypt(json.dumps({"success": False}))
            connection.close()
    except json.JSONDecodeError:
        print("\tFout bij het parsen van login payload.")
        response_payload = encrypt(json.dumps({"success": False}))
    except Exception as e:
        print("\tAndere fout:\n\t->", e)
        response_payload: str = "false"

        
    return response_payload



#
# Verwerk registratie informatie
#
def handle_register(payload: json) -> json:
    try:
        payload = decrypt(payload)
        data = json.loads(payload)
        username = data.get("Username")
        password = data.get("Password")
        email = data.get("Email")

        if not is_valid_email(email):
            print("\tRegistratie mislukt! Ongeldig e-mailadres.")
            response_payload = "Invalid email address"
            return

        connection = connect_to_db()
        if connection:
            cursor = connection.cursor()
            cursor.execute("SELECT * FROM users WHERE username = %s", (username,))
            existing_user = cursor.fetchone()
            print(existing_user)

            if existing_user:
                print("\tGebruikersnaam al in gebruik.")
                response_payload = "Username already exists"
                connection.close()
                return response_payload

            cursor.execute("INSERT INTO users (username, password, email) VALUES (%s, %s, %s)", (username, password, email))
            connection.commit()
            print("\tRegistratie geslaagd!")
            response_payload = encrypt("Registration successful")
            connection.close()
    except json.JSONDecodeError:
        print("\tFout bij het parsen van registratie payload.")
        response_payload = encrypt("Invalid payload format")
    except Exception as e:
        print("\tAndere fout:\n\t->", e)
        response_payload: str = "false"

    return response_payload




#
# Verwerk het uitvoeren van trajecten
#
def handle_execute_command(payload: json) -> str:
    try:
        data = json.loads(payload)
        command = data.get("Command")
        user_id = data.get("UserId")
        print("\texecuting command")

        execute_1_command(command)

        response_payload: str = "true"

    except json.JSONDecodeError:
        print("\tFout bij het parsen van execute_traject payload.")
        response_payload: str = "false"
    except Exception as e:
        print("\tAndere fout:\n\t->", e)
        response_payload: str = "false"

    return response_payload




#
# Verwerk het uitvoeren van trajecten
#
def handle_execute_traject(payload: json) -> str:
    try:
        data = json.loads(payload)
        traject_id = data.get("TrajectId")
        user_id = data.get("UserId")
        cmds_to_exe = None

        connection = connect_to_db()
        if connection:
            cursor = connection.cursor(dictionary=True)
            cursor.execute("SELECT * FROM trajects WHERE id = %s", (traject_id,))
            traject = cursor.fetchone()
            if traject["user_id"] == user_id:
                cursor.execute("SELECT * FROM commands WHERE traject_id = %s", (traject_id,))
                cmds_to_exe = cursor.fetchall()
                for cmd in cmds_to_exe:
                    print("\texecuting command")
                    execute_1_command(cmd)

        else:
            print("\tsomething went wrong")


        response_payload: str = "true"

    except json.JSONDecodeError:
        print("\tFout bij het parsen van execute_traject payload.")
        response_payload: str = "false"
    except Exception as e:
        print("\tAndere fout:\n\t->", e)
        response_payload: str = "false"

    return response_payload




#
# Verwijder trajecten
#
def handle_delete_traject(payload: json) -> str:
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
            print(f"\tTraject {traject_id} verwijderd.")
            response_payload: str = "true"
        else:
            response_payload: str = "false"
    except json.JSONDecodeError:
        print("\tFout bij het parsen van delete_traject payload.")
        response_payload: str = "false"
    except Exception as e:
        print("\tAndere fout:\n\t->", e)
        response_payload: str = "false"

    return response_payload




#
# Laad trajecten voor de gebruiker
#
def handle_load_trajects(payload: json) -> any:
    try:
        data = json.loads(payload)
        print(f"\tOntvangen load trajects payload: {data}")  # Debug info
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

            print(f"\tTrajecten gevonden: {trajects}")  # Debug info
            response_payload: json = json.dumps(trajects, default=datetime_converter)
            connection.close()
        else:
            response_payload: str = "[]"
    except json.JSONDecodeError:
        print("\tFout bij het parsen van load_trajects payload.")
        response_payload: str = "[]"
    except Exception as e:
        print("\tAndere fout:\n\t->", e)
        response_payload: str = "false"

    return response_payload




#
# Verwerk opslaan van trajecten
#
def handle_save_traject(payload: json) -> str:
    try:
        data = json.loads(payload)
        print(f"\tOntvangen traject payload: {data}")  # Debug info

        user_id = data.get("UserId")
        traject = data.get("Traject")
        name = traject.get("Name")
        commands = traject.get("Commands")
        edit = data.get("Edit")
        if edit:
            traject_id = data.get("TrajectId")

        connection = connect_to_db()
        if connection:
            cursor = connection.cursor()
            if not edit:
                cursor.execute("INSERT INTO trajects (name, user_id) VALUES (%s, %s)", (name, user_id))
                traject_id = cursor.lastrowid

                for command in commands:
                    action = command.get("Action")
                    duration = command.get("Duration")
                    speed = command.get("Speed")
                    cursor.execute("INSERT INTO commands (action, duration, traject_id, speed) VALUES (%s, %s, %s, %s)", (action, duration, traject_id, speed))
                
                connection.commit()
                connection.close()
                print(f"\tTraject '{name}' opgeslagen met ID: {traject_id}")
                response_payload: str = "true"

            else:
                cursor.execute("UPDATE trajects SET name=%s WHERE id=%s", (name, traject_id))
                
                cursor.execute("DELETE FROM commands WHERE traject_id=%s", (traject_id, ))

                for command in commands:
                    action = command.get("Action")
                    duration = command.get("Duration")
                    speed = command.get("Speed")
                    cursor.execute("INSERT INTO commands (action, duration, traject_id, speed) VALUES (%s, %s, %s, %s)", (action, duration, traject_id, speed))
                
                connection.commit()
                connection.close()
                print(f"\tTraject '{name}' opgeslagen met ID: {traject_id}")
                response_payload: str = "true"

        else:
            response_payload: str = "false"
    except json.JSONDecodeError:
        print("\tFout bij het parsen van save_traject payload.")
        response_payload: str = "false"
    except Exception as e:
        print("\tAndere fout:\n\t->", e)
        response_payload: str = "false"

    return response_payload




#
# Verwerk opslaan van trajecten
#
def handle_live_measurements() -> json:
    connection = connect_to_db()
    if connection:
        cursor = connection.cursor(dictionary=True)
        cursor.execute("SELECT * FROM measurements")
        measurements = cursor.fetchone()
        measurements["timestamp"] = measurements["timestamp"].isoformat()
        response_payload: json = json.dumps(measurements, default=datetime_converter)
    else:
        response_payload: json = json.dumps("false")
    return response_payload


#
# all measurements opsturen
#
def handle_all_measurements(payload: json) -> json:
    try:
        data = json.loads(payload)
        startDate = data["startTimestamp"]
        endDate = data["endTimestamp"]
        min = data["startValue"]
        max = data["endValue"]

        connection = connect_to_db()
        if connection:
            cursor = connection.cursor(dictionary=True)
            cursor.execute("SELECT * FROM measurements WHERE (value BETWEEN %s AND %s) AND (timestamp BETWEEN %s AND %s)", (min, max, startDate, endDate))
            measurements = cursor.fetchall()
            for measurement in measurements:
                measurement["timestamp"] = measurement["timestamp"].isoformat()
            response_payload: json = json.dumps(measurements, default=datetime_converter)
    except json.JSONDecodeError:
        print("\tFout bij het parsen van live_measurements payload.")
        response_payload: json = json.dumps("false")
    except Exception as e:
        print("\tAndere fout:\n\t->", e)
        response_payload: json = json.dumps("false")

    return response_payload




#
# Sensor reading
#
def get_sensor_reading() -> json:
    try:
        response = UART_get_reading()
        current_datetime = dt.now().strftime('%Y-%m-%d %H:%M:%S') 

        connection = connect_to_db()
        if connection:
            cursor = connection.cursor(dictionary=True)
            cursor.execute("INSERT INTO measurements (Value, Timestamp) VALUES (%s, %s)", (response, current_datetime))
            id = cursor.lastrowid

            connection.commit()
            connection.close()

        payload: json = json.dumps({"Id": id, "Value": response, "Timestamp": current_datetime})

    except json.JSONDecodeError:
        print("\tFout bij het parsen van live_measurements payload.")
        response_payload: json = json.dumps("false")
    except Exception as e:
        print("\tAndere fout:\n\t->", e)
        response_payload: json = json.dumps("false")

    return payload




#
# 1 commando uitvoeren
#
def execute_1_command(cmd):
    action = cmd.get("Action")
    if action == None:
        action = cmd.get("action")

    duration = cmd.get("Duration")
    if duration == None:
        duration = cmd.get("duration")

    speed = cmd.get("Speed")
    if speed == None:
        speed = cmd.get("speed")
    print(speed)

    bits: str = ""

    if action == "Vooruit":
        bits = "001"
    elif action == "Achteruit":
        bits = "000"
    elif action == "Links":
        bits = "010"
    elif action == "Rechts":
        bits = "011"
    elif action == "Links_draaien":
        bits = "101"
    elif action == "Rechts_draaien":
        bits = "100"
    elif action == "Measurement":
        bits = "110"
    elif action == "Idle":
        bits = "111"

    bits += f"{int(speed/100*32):05b}"
    print(bits)

    if len(bits) == 8:
        UART_send(bits)
        print(f"\t\n-> i'm sending {action} over UART B-)")
        sleep(duration + 0.010) 
        UART_send("11100000")       # to idle
    else:
        print(f"\t\n-> Couldn't send {action}")



#
# Functie om datetime-objecten te converteren naar strings
#
def datetime_converter(o):
    if isinstance(o, datetime.datetime):
        return o.isoformat()  # Of een ander formaat zoals o.strftime('%Y-%m-%d %H:%M:%S')
    
    
def datetime_converter2(o):
    if isinstance(o, timestamp.datetime):
        return o.isoformat()  # Of een ander formaat zoals o.strftime('%Y-%m-%d %H:%M:%S')

#
# Functie voor het valideren van e-mailadressen
# 
def is_valid_email(email):
    email_regex = r'^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$'
    return re.match(email_regex, email) is not None