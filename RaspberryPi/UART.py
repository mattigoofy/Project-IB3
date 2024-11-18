import serial
import sys
import getopt

BAUT_RATE = 9600
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
        serialPort = serial.Serial(PORT, BAUT_RATE, timeout=5)
        print(data_bytes)
        serialPort.write(data_bytes)
        serialPort.close()
    except ValueError:
        print("Something went wrong")
        return



def UART_read():
    try:
        serialPort = serial.Serial(PORT, BAUT_RATE, timeout=5)
        din = serialPort.read()
        serialPort.close()
        return din
    except ValueError:
        print("Something went wrong")
        return
    
if __name__ == "__main__":
    period = 0.5/(104*10**-6)
    print(int(period/2/8))
    UART_send(b'\x55')
    print("Sending data...")
    while True:
        print("high")
        for i in range(0, int(period/2/8)):
            UART_send(b'\x01')
        print("low")
        for i in range(0, int(period/2/8)):
            UART_send(b'\x00')
    UART_send(b'\xAA')

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
