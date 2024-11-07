import paho.mqtt.client as mqtt
import mysql.connector
import json
import re  # Voor e-mailvalidatie
from testCrypt import *

# MQTT-instellingen
BROKER = "localhost"
PORT = 1883

# MariaDB-instellingen
DB_HOST = "localhost"
DB_USER = "wagentje_user"
DB_PASSWORD = "robot"
DB_NAME = "wagentje_db"

# Verbinding maken met MariaDB
def connect_to_db():
    try:
        connection = mysql.connector.connect(
            host=DB_HOST,
            user=DB_USER,
            password=DB_PASSWORD,
            database=DB_NAME
        )
        return connection
    except mysql.connector.Error as err:
        print(f"Fout bij het verbinden met de database: {err}")
        return None

# Callback wanneer de client verbinding maakt met de broker
def on_connect(client, userdata, flags, rc):
    print(f"Verbonden met broker. Resultaatcode: {rc}")
    # Abonneer op de relevante topics
    client.subscribe("raspberrypi/login")
    client.subscribe("raspberrypi/register")
    client.subscribe("raspberrypi/execute_traject")
    client.subscribe("raspberrypi/delete_traject")
    client.subscribe("raspberrypi/load_trajects")
    client.subscribe("raspberrypi/save_traject")

# Callback voor wanneer een bericht ontvangen wordt
def on_message(client, userdata, msg):
    topic = msg.topic
    payload = msg.payload.decode("utf-8")
    print(f"Bericht ontvangen op topic {topic}: {payload}")
    
    if topic == "raspberrypi/login":
        handle_login(payload)
    elif topic == "raspberrypi/register":
        handle_register(payload)
    elif topic == "raspberrypi/execute_traject":
        handle_execute_traject(payload)
    elif topic == "raspberrypi/delete_traject":
        handle_delete_traject(payload)
    elif topic == "raspberrypi/load_trajects":
        handle_load_trajects(payload)
    elif topic == "raspberrypi/save_traject":
        handle_save_traject(payload)

# Functie voor het valideren van e-mailadressen
def is_valid_email(email):
    email_regex = r'^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$'
    return re.match(email_regex, email) is not None

# Verwerk registratie informatie
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

# Verwerk login informatie
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
                    response_payload = json.dumps({"success": True, "userId": user_id, "username": username})
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


# Verwerk opslaan van trajecten
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

# Verwerk het uitvoeren van trajecten
def handle_execute_traject(payload):
    try:
        data = json.loads(payload)
        # Logica toevoegen voor uitvoeren van traject met `traject_id` en `user_id`
        client.publish("raspberrypi/execute_traject/response", "true")
    except json.JSONDecodeError:
        print("Fout bij het parsen van execute_traject payload.")
        client.publish("raspberrypi/execute_traject/response", "false")

# Verwijder trajecten
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

# Functie om datetime-objecten te converteren naar strings
def datetime_converter(o):
    if isinstance(o, datetime.datetime):
        return o.isoformat()  # Of een ander formaat zoals o.strftime('%Y-%m-%d %H:%M:%S')
    
# Laad trajecten voor de gebruiker
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

# Setup MQTT-client
client = mqtt.Client()
client.on_connect = on_connect
client.on_message = on_message

# Verbind met de MQTT-broker
client.connect(BROKER, PORT, 60)

# Blijf luisteren naar berichten
client.loop_forever()
