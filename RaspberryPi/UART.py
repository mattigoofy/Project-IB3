import serial
import sys
import getopt

BAUD_RATE = 9600
PORT = "/dev/ttyUSB1"


def UART_send(data_binary):
    try:
        data_bytes = int(data_binary, 2).to_bytes(1, byteorder='big')
    except Exception:
        print("Invalid binary string format")
        return
    
    try:
        serialPort = serial.Serial(PORT, BAUD_RATE, timeout=5)
        serialPort.write(data_bytes)
        serialPort.close()
    except Exception as e:
        print("Something went wrong:", e)
        return



def UART_read():
    try:
        serialPort = serial.Serial(PORT, BAUD_RATE, timeout=5)
        din = serialPort.read()
        serialPort.close()
        return din
    except Exception as e:
        print("Something went wrong:", e)
        return
        

def UART_get_reading():
    try:
        with serial.Serial(PORT, BAUD_RATE, timeout=0.1) as serialPort:
            data_bytes = int("11010000", 2).to_bytes(1, byteorder='big')

            serialPort.write(data_bytes)

            response = serialPort.read(2)  # Read 2 bytes
            if len(response) == 2:
                result = int.from_bytes(response, byteorder='big')
                return result
            else:
                print("Incomplete data received.")
                return None
    except ValueError as e:
        print(f"Error: {e}")
        return None
    except serial.SerialException as e:
        print(f"Serial error: {e}")
        return None


if __name__ == "__main__":
    # response = UART_send_and_read("11010000")
    response = UART_get_reading()
    if response:
        print(f"Response: {response}")
    else:
        print("No response received.")