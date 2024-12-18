import serial
import sys
import getopt

BAUD_RATE = 9600
# PORT = "/dev/ttyS0"
PORT = "COM12"

def UART_send(data_binary):
    # try:
    #     data_bytes = int(data_binary, 2).to_bytes(1, byteorder='big')
    # except ValueError:
    #     print("Invalid binary string format")
    #     return
    data_bytes = data_binary
    
    try:
        serialPort = serial.Serial(PORT, BAUD_RATE, timeout=5)
        print(data_bytes)
        serialPort.write(data_bytes)
        serialPort.close()
    except ValueError:
        print("Something went wrong")
        return

def UART_send_bin(data_binary):
    try:
        data_bytes = int(data_binary, 2).to_bytes(1, byteorder='big')
    except ValueError:
        print("Invalid binary string format")
        return
    # data_bytes = data_binary

    try:
        serialPort = serial.Serial(PORT, BAUD_RATE, timeout=5)
        print(data_bytes)
        serialPort.write(data_bytes)
        serialPort.close()
    except ValueError:
        print("Something went wrong")
        return



def UART_read():
    try:
        serialPort = serial.Serial(PORT, BAUD_RATE, timeout=5)
        din = serialPort.read()
        serialPort.close()
        return din
    except ValueError:
        print("Something went wrong")
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
    # period = 0.5/(104*10**-6)
    # print(int(period/2/8))
    # UART_send(b'\x55')
    # print("Sending data...")
    # while True:
    #     print("high")
    #     for i in range(0, int(period/2/8)):
    #         UART_send(b'\x01')
    #     print("low")
    #     for i in range(0, int(period/2/8)):
    #         UART_send(b'\x00')
    # UART_send(b'\xAA')

    # print("Sending data...")
    # UART_send("01010101")
    # UART_send("00110011")
    # UART_send("11001100")
    # UART_send("00111100")
    # UART_send("10101010")
    # print("Send complete")

    # print("Sending data...")
    # UART_send(b'\x55')
    # UART_send(b'\x01')
    # UART_send(b'\x02')
    # UART_send(b'\x03')
    # UART_send(b'\xAA')
    # print("Send complete")
