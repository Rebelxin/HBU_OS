import sys
import socket
from PyQt5.QtWidgets import QApplication, QMainWindow, QVBoxLayout, QPushButton, QTextEdit, QWidget
from PyQt5.QtCore import QThread, pyqtSignal

import os_ui


class ClientThread(QThread):
    response_received = pyqtSignal(str)  # 用于发送接收到的数据到主线程
    error_occurred = pyqtSignal(str)    # 用于发送错误信息到主线程

    def __init__(self, host, port, message):
        super().__init__()
        self.host = host
        self.port = port
        self.message = message


    def run(self):
        try:
            # 创建 TCP 连接
            with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as client:
                client.connect((self.host, self.port))
                client.sendall(self.message.encode("utf-8"))
                response = client.recv(1024).decode("utf-8")
                self.response_received.emit(response)  # 将结果发送回主线程
        except Exception as e:
            self.error_occurred.emit(str(e))


class ClientWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("PyQt Multithread TCP Client")

        self.mainWindow = os_ui.MainWindow(self)
        self.mainWindow.show()

        # UI Setup
        self.text_edit = QTextEdit(self)
        self.button = QPushButton("Send to C#", self)
        self.button.clicked.connect(self.send_to_csharp)

        layout = QVBoxLayout()
        layout.addWidget(self.text_edit)
        layout.addWidget(self.button)

        container = QWidget()
        container.setLayout(layout)
        self.setCentralWidget(container)

        # TCP 客户端设置
        self.host = "127.0.0.1"  # 本地回环地址
        self.port = 5000         # 与 C# 服务端一致
        self.worker = None

    def send_to_csharp(self,data):
        self.text_edit.append("发送中...")
        self.worker = ClientThread(self.host, self.port, data)
        self.worker.response_received.connect(self.handle_response)
        self.worker.error_occurred.connect(self.handle_error)
        self.worker.start()  # 开启线程

    def handle_response(self, response):
        self.text_edit.append(f"服务端回复: {response}")

    def handle_error(self, error):
        self.text_edit.append(f"通信错误: {error}")

    def get_command(self):
        self.mainWindow.cmd_win.command

    def commandSendEvent(self,command):
        print(f"成功触发{self.windowTitle()}属性")
        self.send_to_csharp(command)

if __name__ == "__main__":
    app = QApplication(sys.argv)
    window = ClientWindow()
    window.show()
    app.exec_()



