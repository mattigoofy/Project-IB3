import mysql.connector

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