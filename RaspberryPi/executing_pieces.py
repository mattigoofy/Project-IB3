from UART import *
from time import sleep

# Verwerk het uitvoeren van trajecten
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



# Verwerk het uitvoeren van trajecten
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


