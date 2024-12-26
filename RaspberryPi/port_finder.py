import serial.tools.list_ports as s

ports = s.comports()
for port in ports:
    print(port.device)