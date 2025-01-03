import json
import sys
import socket
from PyQt5.QtWidgets import QApplication, QMainWindow, QVBoxLayout, QPushButton, QTextEdit, QWidget, QTreeWidgetItem
from PyQt5.QtCore import QThread, pyqtSignal

import os_ui
#asdadasdada

class ClientConnect():
    def __init__(self, host, port, message):
        super().__init__()
        self.host = host
        self.port = port
        self.message = message


    def send(self):
        # 创建 TCP 连接
        try:
            with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as client:
                client.connect((self.host, self.port))
                client.sendall(self.message.encode("utf-8"))

                buffer_size = 4096  # 设置缓冲区大小
                response = ""

                while True:
                    # 一次读取 buffer_size 大小的数据
                    chunk = client.recv(buffer_size).decode("utf-8")
                    if not chunk:  # 如果读取到空数据，说明传输结束
                        break
                    response += chunk

                return response
        except Exception as e:
            raise e


class ClientWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("Server response")

        self.mainWindow = os_ui.MainWindow(self)
        self.mainWindow.show()
        self.requestType = ""

        # UI Setup
        self.text_edit = QTextEdit(self)
        self.text_edit.setReadOnly(True)

        layout = QVBoxLayout()
        layout.addWidget(self.text_edit)

        container = QWidget()
        container.setLayout(layout)
        self.setCentralWidget(container)

        # TCP 客户端设置
        self.host = "127.0.0.1"  # 本地回环地址
        self.port = 5000         # 与 C# 服务端一致
        self.worker = None

    def send_to_csharp(self,data):
        self.text_edit.append("发送中...")
        self.worker = ClientConnect(self.host, self.port, data)

        try:
            response = self.worker.send()
        except Exception as e:
            raise e
        self.handle_response(response)



    def handle_response(self, response):
        self.text_edit.append(f"服务端回复: {response}")
        response = json.loads(response)
        if self.requestType == "GET_FILE_TREE":
            self.ConstructFileTree(response["Data"])
        if self.requestType == "CREATE_FILE":
            if response["Status"] == "Error":
                print(response["Message"])

    def RequestSendEvent(self, Requests):
        for request in Requests:
            self.requestType = json.loads(request)["RequestType"]
            self.send_to_csharp(request)


    def ConstructFileTree(self,data):
        print("construct")
        json_data = data
        self.mainWindow.dir_win.root.takeChildren()
        self.mainWindow.dir_win.root.setText(0, json_data["Name"])
        for child in json_data.get("Children", []):
            self.fileTreeAddItem(self.mainWindow.dir_win.root, child)

    def fileTreeAddItem(self,parent, node):
        # 创建当前节点
        tree_item = QTreeWidgetItem(parent)
        tree_item.setText(0, node["Name"])  # 设置第一列显示 Name
        if node.get("IsDirectory", False):
            tree_item.setIcon(0,self.mainWindow.dir_win.icon_folder)
        else:
            tree_item.setIcon(0,self.mainWindow.dir_win.icon_file)
        # 如果有子节点，递归添加
        for child in node.get("Children", []):
            self.fileTreeAddItem(tree_item, child)



if __name__ == "__main__":
    app = QApplication(sys.argv)
    window = ClientWindow()
    window.show()
    app.exec_()



