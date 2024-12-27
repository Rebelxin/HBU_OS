import json


class JSONRequest:
    def __init__(self,requestType,data):
        self.requestType = requestType#asdadasdada
        self.data = data

    def getJSON(self):
        data = {
            "RequestType":self.requestType,
            "Data":self.data
        }
        return json.dumps(data)