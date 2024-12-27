# 192表示0，319表示127
# 320(ŀ)表示系统占用，321(Ł)表示end，其它特殊符号顺次后延
# NNNTTRSL N:目录名，T：拓展名，R：读写属性，S：起始盘块号，L：文件长度（目录没有长度）
# print(chr(192))
"""
a = "0"
b = a if len(a) >= 3 else a + "À"* (3 - len(a))
print(b)
"""


class Disk:
    def __init__(self):
        f = open("./disk.txt", encoding="utf8")  # 打开磁盘文件
        self.content = f.readlines()  # 读取文件内容
        f.close()  # 关闭文件
        self.current_dir = [""]  # 当前目录，初始化为根目录

    """
    +------------------------------+
    |         功能细分函数          |
    +------------------------------+
    """

    def operate(self, operation, info):  # 外部调用，用以接收命令并调用函数
        method = getattr(self, operation, None)
        if method:
            result = method(info)
            return result
        else:
            print("no operation")

    def absolutePath(self, info):  # 接收绝对路径或相对路径，返回绝对路径
        path = self.current_dir
        print("disk-absolutePath-path", path)
        info = info[:-1] if info[-1] == "" and len(info) != 1 else info
        print("disk-absolutePath-info", info)
        for i in info:
            if i == ".":
                continue
            elif i == "..":
                path = path[:-1]
                # print(len(path))
                if len(path) == 0:
                    path.append("")
            elif i == "":
                path.clear()
                path.append(i)
            else:
                path.append(i)
        print("disk-absolutePath-path", path)
        return path

    def blockNumbers(self, path):  # 查找特定文件占用的空间块号
        print("disk-blockNumbers-path", path)
        path = self.absolutePath(path)  # path: ["", dir1, dir2, ... ,target]
        print("disk-blockNumbers-path", path)
        b_p = 2  # b_p: block_pointer 指向当前所在的块号
        f_n_p = 1  # f_n_p: file_name_pointer 指向当前所在的path中的文件名，作为path列表的索引
        num_lst = []

        if path == [""]:
            num_lst = [2]
            return num_lst

        while True:
            for i in range(8):
                print("disk-blockNumbers-f_n_p", f_n_p)
                if "." in path[f_n_p]:
                    file_name = ".".join(path[f_n_p].split(".")[:-1])
                    extension_name = path[f_n_p].split(".")[-1]
                    file_name = (
                        file_name
                        if len(file_name) >= 3
                        else file_name + "À" * (3 - len(file_name))
                    )
                    extension_name = (
                        extension_name
                        if len(extension_name) >= 2
                        else extension_name + "À" * (2 - len(extension_name))
                    )
                    full_name = file_name + extension_name
                if (
                    full_name == self.content[b_p][0 + i * 8 : 5 + i * 8]
                ):  # 匹配文件名，匹配成功
                    if f_n_p == len(path) - 1:  # 是最终文件
                        # 获取自身首块号, 开始在前两行查找块号
                        b_p = eval(self.content[b_p][6 + i * 8])
                        while True:
                            num_lst.append(b_p)
                            if self.content[b_p // 64][b_p % 64] == "Ł":
                                break
                            else:
                                b_p = ord(self.content[b_p // 64][b_p % 64]) - 192
                        return num_lst

                    else:  # 非最终文件
                        f_n_p += 1
                        # 获取自身首块号, 下一轮进入自己的块空间
                        b_p = eval(self.content[b_p][6 + i * 8])
                else:  # 匹配文件名，匹配失败
                    if b_p == 2:  # 不可能有后续块
                        return num_lst
                    else:  # 有可能存在后续块
                        b_p = self.content[b_p // 64][b_p % 64]
                        if b_p == "Ł":  # “Ł”表示end，即为不存在后续块
                            return num_lst
                        else:  # 无后续块
                            b_p = self.content[ord(b_p)][6 + i * 8]

        """
        根据块号指针开始循环
            匹配文件名，匹配成功：
                是最终文件：
                    进入块号循环，获取块号列表，终止循环，返回块号列表
                非最终文件：
                        更改循环块号以及寻找的文件名称指针，准备下一轮循环
            匹配文件名，匹配失败：
                有后续块：
                    更改循环块号，准备下一轮循环
                无后续块：
                    终止循环，返回空列表
        """

    def byteAvailable(self, path):
        self.blockNumbers(path)#asdadasdada#asdadasdada
        pass

    def blcokAvailable(self, number):  #  查找空闲块，number为需要的块数
        empty_blocks = []
        for i in range(128):
            if len(empty_blocks) >= number:
                return empty_blocks
            if self.content[i // 64][i % 64] == "À":
                empty_blocks.append(i)
        return empty_blocks

    def addFile(self, whole_file_name, fcb_pos, start_pos, self_pos):
        print("disk-addFile-whole_file_name", whole_file_name)
        if "." in whole_file_name:  # 判断类型
            file_length = "1"  # 普通文件
        else:
            file_length = "0"  # 目录文件

        file_name = ".".join(whole_file_name.split(".")[:-1])
        extension_name = whole_file_name.split(".")[-1]

        fcb = (
            (
                file_name
                if len(file_name) >= 3
                else file_name + "À" * (3 - len(file_name))
            )
            + (
                extension_name
                if len(extension_name) >= 2
                else extension_name + "À" * (2 - len(extension_name))
            )
            + "W"
            + chr(self_pos + 192)
            + file_length
        )
        # 添加FCB
        self.content[fcb_pos] = (
            self.content[fcb_pos][:start_pos]
            + fcb
            + self.content[fcb_pos][start_pos + 8 :]
        )
        # 设置块状态标识符为"Ł"，表示占用
        self.setBlockTag(self_pos, "Ł")

    def setBlockTag(self, self_pos, tag):  # 更改块占用标识符
        block_num = self_pos // 64
        block_pos = self_pos % 64
        self.content[block_num] = (
            self.content[block_num][:block_pos]
            + tag
            + self.content[block_num][block_pos + len(tag) :]
        )

    """
    +------------------------------+
    |         磁盘指令函数          |
    +------------------------------+
    """

    def cd(self, info):  # 切换目录
        self.current_dir = self.absolutePath(info)
        return self.current_dir

    def create(self, info):
        empty_blocks = self.blcokAvailable(2)
        if len(empty_blocks) < 1:
            return "磁盘空间不足"
        whole_path = self.absolutePath(info)
        print("disk-create-whole_path", whole_path)
        file_location = whole_path[:-1]
        print("disk-create-file_location", file_location)
        whole_file_name = whole_path[-1]
        start_pos = None
        print("disk-create-file_location", file_location)
        result = self.blockNumbers(file_location)
        print("disk-create-result", result)
        for i in range(len(result)):
            if "ÀÀÀÀÀÀÀÀ" in self.content[result[i]]:  # 父级目录有空间存放FCB
                for j in range(8):
                    if self.content[result[i]][j * 8 : j * 8 + 8] == "ÀÀÀÀÀÀÀÀ":
                        start_pos = j * 8
                        break
                break
            else:  # 父级目录无空间存放FCB
                if len(empty_blocks) < 2:
                    return "磁盘空间不足"
                start_pos = 0
                self.addFile(whole_file_name, result[i], start_pos, empty_blocks[1])
                # 父级目录更改占用标识符
                self.setBlockTag(empty_blocks[0], "Ł")
                self.setBlockTag(result[i], chr(empty_blocks[0] + 192))

        for i in range(16):
            print(self.content[i])
        return "创建成功"
