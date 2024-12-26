from Crypto.Cipher import AES
from Crypto.Util.Padding import pad, unpad
import base64

key = b'1234567890123456'  # 16-byte key
iv = b'1234567890123456'   # 16-byte IV

def encrypt(plain_text):
    cipher = AES.new(key, AES.MODE_CBC, iv)
    padded_text = pad(plain_text.encode(), AES.block_size)
    encrypted = cipher.encrypt(padded_text)
    return base64.b64encode(encrypted).decode()

def decrypt(cipher_text):
    cipher = AES.new(key, AES.MODE_CBC, iv)
    decrypted = cipher.decrypt(base64.b64decode(cipher_text))
    return unpad(decrypted, AES.block_size).decode()

# Example Usage
if __name__ == "__main__":
    message = "TestSecretstring"
    encrypted_message = encrypt(message)
    print("Encrypted:", encrypted_message)
    # decrypted_message = decrypt(encrypted_message)
    decrypted_message = decrypt(input("Input: "))
    print("Decrypted:", decrypted_message)
