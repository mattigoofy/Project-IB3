from datetime import datetime

def get_sensor_reading() -> json:
    try:
        response = UART_get_reading()
        current_datetime = datetime.now()

        connection = connect_to_db()
        if connection:
            cursor = connection.cursor(dictionary=True)
            cursor.execute("INSERT INTO distace_measurements (Value, Timestamp) VALUES (%s, %s)", (response, current_datetime))
            id = cursor.lastrowid

            connection.commit()
            connection.close()

        payload: json = json.dumps({"Id": id, "Value": response, "Timestamp": current_datetime})

    except:
        print("failed")
        payload: json = json.dumps({"Sucecess": False})

    return payload