from handlers import *
import paho.mqtt.client as mqtt
import threading
import time

# MQTT-instellingen
BROKER: str = "localhost"
PORT: int = 1883

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
    client.subscribe("raspberrypi/all_measurements")

# Callback voor wanneer een bericht ontvangen wordt
def on_message(client, userdata, msg):
    topic = msg.topic
    payload = msg.payload.decode("utf-8")
    print(f"\n\n--------------\nBericht ontvangen op topic {topic}: \n--------------\n->payload: {payload}")
    
    if topic == "raspberrypi/login":
        response_payload: json = handle_login(payload)
        client.publish("raspberrypi/login/response", response_payload)

    elif topic == "raspberrypi/register":
        response_payload: json = handle_register(payload)
        client.publish("raspberrypi/register/response", response_payload)

    elif topic == "raspberrypi/execute_command":
        response_payload: str = handle_execute_command(payload)
        client.publish("raspberrypi/execute_command/response", response_payload)

    elif topic == "raspberrypi/execute_traject":
        response_payload: str = handle_execute_traject(payload)
        client.publish("raspberrypi/lexecute_traject/response", response_payload)

    elif topic == "raspberrypi/delete_traject":
        response_payload: str = handle_delete_traject(payload)
        client.publish("raspberrypi/delete_traject/response", response_payload)

    elif topic == "raspberrypi/load_trajects":
        response_payload: any = handle_load_trajects(payload)
        client.publish("raspberrypi/load_trajects/response", response_payload)

    elif topic == "raspberrypi/save_traject":
        response_payload: str = handle_save_traject(payload)
        client.publish("raspberrypi/save_traject/response", response_payload)

    elif topic == "raspberrypi/live_measurements":
        response_payload: json = handle_live_measurements(payload)
        client.publish("raspberrypi/live_measurements/response", response_payload)

    elif topic == "raspberrypi/all_measurements":
        response_payload: json = handle_all_measurements(payload)
        client.publish("raspberrypi/all_measurements/response", response_payload)



def publish_constant_data(client):
    while True:
        response_payload = get_sensor_reading()
        client.publish("raspberrypi/measurement", response_payload)
        time.sleep(5)  # Wacht 5 seconden voor de volgende publicatie

# Setup MQTT-client
client = mqtt.Client()
client.on_connect = on_connect
client.on_message = on_message

# Verbind met de MQTT-broker
client.connect(BROKER, PORT, 60)

threading.Thread(target=publish_constant_data, args=(client,), daemon=True).start()

# Blijf luisteren naar berichten
client.loop_forever()
