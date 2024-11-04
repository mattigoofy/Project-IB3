import serial
import sys
import getopt

BAUT_RATE = 9600
PORT = "/dev/ttyS0"


def UART_send(data_binary):
    try:
        data_bytes = int(data_binary, 2).to_bytes(1, byteorder='big')
    except ValueError:
        print("Invalid binary string format")
        return
    
    try:
        serialPort = serial.Serial(PORT, BAUT_RATE, timeout=5)
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
