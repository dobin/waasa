import yaml
import json

with open('info.yaml', 'r') as file:
    configuration = yaml.safe_load(file)

with open('info.json', 'w') as json_file:
    json.dump(configuration['Extensions'], json_file)
