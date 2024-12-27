import re


class CmdFormatChecker:
    def check(self, cmd: str):
        try:
            cmd_lst = re.split(r"\s+", cmd)
        except Exception as e:
            return False, "None", [None]

        method = getattr(self, cmd_lst[0], None)
        if method:
            result = method(cmd_lst)
            return result
        else:
            return False, "None", ["未知命令"]

    def cd(self, cmd_lst):
        if len(cmd_lst) != 2 or cmd_lst[1] == "":
            return False, "cd", ["命令格式错误"]

        path = re.split(r"/+", cmd_lst[1])
        for i in path:
            if "." in i and i != "." and i != "..":
                return False, "cd", [f"‘{i}’不是目录"]

        return True, "cd", path

    def create(self, cmd_lst):
        # print("create checking")
        if len(cmd_lst) != 2 or cmd_lst[1] == "":
            return False, "create", ["命令格式错误"]

        path = re.split(r"/+", cmd_lst[1])
        if "." not in path[-1]:
            return False, "create", ["‘create’不能创建目录，请使用‘mkdir’"]

        file_name = ".".join(path[-1].split(".")[:-1])
        extension_name = path[-1].split(".")[-1]
        if len(file_name) < 1 or len(file_name) > 3:
            return (
                False,
                "create",
                [f"‘{file_name}.{extension_name}’：文件名应为 1~3 字符"],
            )

        if len(extension_name) < 1 or len(extension_name) > 2:
            return (
                False,
                "create",
                [f"‘{file_name}.{extension_name}’：拓展名应为 1~2 字符"],
            )

        for i in path[:-1]:
            if "." in i and i != "." and i != "..":
                return False, "create", [f"‘{i}’不是目录"]

        return True, "create", path

    def delete(self, text):
        return True, "delete命令"

    def makdir(self, text):
        return True, "mkdidr命令"

    def deldir(self, text):
        return True, "deldir命令"

    def copy(self, text):
        return True, "copy命令"

    def move(self, text):
        return True, "move命令"

    def type(self, text):
        return True, "type命令"

    def edit(self, text):
        return True, "edit命令"
