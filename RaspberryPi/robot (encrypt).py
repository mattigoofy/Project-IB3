import paho.mqtt.client as mqtt
import mysql.connector
import json
import re  # Voor e-mailvalidatie
from testCrypt import *
from handlers import *

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
    client.subscribe("raspberrypi/execute_command")
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
    elif topic == "raspberrypi/execute_command":
        handle_execute_command(payload)
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

# Functie om datetime-objecten te converteren naar strings
def datetime_converter(o):
    if isinstance(o, datetime.datetime):
        return o.isoformat()  # Of een ander formaat zoals o.strftime('%Y-%m-%d %H:%M:%S')

# Setup MQTT-client
client = mqtt.Client()
client.on_connect = on_connect
client.on_message = on_message

# Verbind met de MQTT-broker
client.connect(BROKER, PORT, 60)

# Blijf luisteren naar berichten
client.loop_forever()
