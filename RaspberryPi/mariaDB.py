import mariadb
import sys

try:
    conn = mariadb.connect(
        user="wachentje_user",
        password="jeSterkeWachtwoord",
        # host="192.0.2.1",
        # port=3306,
        # database="employees"

    )
except mariadb.Error as e:
    print(f"Error connecting to MariaDB Platform: {e}")
    sys.exit(1)

cur = conn.cursor()


def insertSensorData(value, time):
    cur.execute("INSERT INTO sensordata (value, time) VALUES", value, time)

def insertInlogData(username, password):
    cur.execute("INSERT INTO inlogdata (username, password) VALUES (?,?)", username, password)
    
