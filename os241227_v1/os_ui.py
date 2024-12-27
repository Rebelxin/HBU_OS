import sys

from PyQt5.QtWidgets import QMainWindow, QApplication, QSplitter, QWidget, QVBoxLayout
from PyQt5.QtGui import QPalette
from PyQt5.QtCore import Qt, QUrl

from terminal import Terminal
from diagram import Diagram
from directory import Directory
from cmd_checker import CmdFormatChecker
from disk import Disk


class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("OS模拟")
        self.init_width = int(QApplication.primaryScreen().size().width() * 0.6)
        self.init_height = int(self.init_width * 0.65)
        self.resize(self.init_width, self.init_height)
        self.initUI()

    def initUI(self):
        self.h_splitter = QSplitter(self)
        self.left_v_splitter = QSplitter(self)
        self.left_top_area = QWidget(self)
        self.left_bottom_area = QWidget(self)
        self.right_area = QWidget(self)
        self.left_top_layout = QVBoxLayout(self.left_top_area)
        self.left_bottom_layout = QVBoxLayout(self.left_bottom_area)
        self.right_layout = QVBoxLayout(self.right_area)

        self.left_v_splitter.setOrientation(Qt.Vertical)

        self.left_top_area.setObjectName("left_top_area")
        self.left_top_area.setStyleSheet(
            "QWidget#left_top_area { background-color: #AA4444; }"
        )
        self.left_bottom_area.setObjectName("left_bottom_area")
        self.left_bottom_area.setStyleSheet(
            "QWidget#left_bottom_area { background-color: #44AA44; }"
        )
        self.right_area.setObjectName("right_area")
        self.right_area.setStyleSheet(
            "QWidget#right_area { background-color: #4444AA; }"
        )

        color = self.palette().color(QPalette.Window).name()
        self.h_splitter.setObjectName("h_splitter")
        self.h_splitter.setStyleSheet(
            f"""QSplitter#h_splitter::handle {{ background: {color}; }}"""
        )
        self.left_v_splitter.setObjectName("left_v_splitter")
        self.left_v_splitter.setStyleSheet(
            f"""QSplitter#left_v_splitter::handle {{ background: {color}; }}"""
        )

        self.setCentralWidget(self.h_splitter)
        self.h_splitter.addWidget(self.left_v_splitter)
        self.left_v_splitter.addWidget(self.left_top_area)
        self.left_v_splitter.addWidget(self.left_bottom_area)
        self.h_splitter.addWidget(self.right_area)

        self.h_splitter.setSizes([300, 200])  # 初始大小可以根据需要设定
        self.left_v_splitter.setSizes([200, 300])  # 初始大小可以根据需要设定

        self.dir_win = Directory()
        self.left_top_layout.addWidget(self.dir_win)

        self.cmd_win = Terminal()
        self.left_bottom_layout.addWidget(self.cmd_win)

        self.diag_win = Diagram()
        self.right_layout.addWidget(self.diag_win)

        self.cmd_checker = CmdFormatChecker()
        self.disk = Disk()

        self.cmd_win.submit.connect(self.getCmd)

    def getCmd(self, cmd: str):
        # print("check")
        status, cmd_prompt, info = self.cmd_checker.check(cmd)
        # print(status, cmd_prompt, info)
        # cmd_checker只负责细致检查命令格式，不检查命令是否能执行成功
        # status: bool, True表示命令格式正确，False表示命令格式错误
        # cmd_prompt: str, 命令提示符
        # info: lst, 内容，命令格式正确时为路径等必要信息，命令格式错误时则为报错信息

        if status:
            result = self.disk.operate(cmd_prompt, info)
            if result:
                self.respondCmd(cmd_prompt, result)

    def respondCmd(self, cmd_prompt, result):
        method = getattr(self, cmd_prompt, None)
        if method:
            method(result)

    def cd(self, result):
        self.cmd_win.title = "/".join(result) + "> " if result != [""] else "/> "
        print(result)


def main():
    app = QApplication(sys.argv)
    window = MainWindow()
    window.show()
    sys.exit(app.exec_())


if __name__ == "__main__":
    main()