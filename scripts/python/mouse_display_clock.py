import json
import requests
from time import sleep
import datetime

app = 'CLOCK'
display_name = 'Clock'
time_event = 'TIME'
bzzz_event = 'BZZZ'

corePropsPath = '/ProgramData/SteelSeries/SteelSeries Engine 3/coreProps.json'

with open(corePropsPath, 'r') as file:
    gamesense_url = json.load(file)['address']

def register_clock():
    clock_metadata = {
        'game': app,
        'game_display_name': display_name,
    }
    requests.post(f'http://{gamesense_url}/game_metadata', json=clock_metadata)

def bind_clock_event():
    clock_handler = {
        'game': app,
        'event': time_event,
        'icon_id': 15,
        'handlers': [
            {
               'device-type': 'screened',
               'mode': 'screen',
               'zone': 'one',
			   "datas": [
                   {
                       "icon-id": 15,
                       'has-text': True,
                       'length-millis': 1100
                   }
               ]
            }
        ]
    }
    requests.post(f'http://{gamesense_url}/bind_game_event', json=clock_handler)

def bind_bzzz_event():
    clock_handler = {
        'game': app,
        'event': bzzz_event,
        'handlers': [
            {
               'device-type': 'tactile',
			   'mode': 'vibrate',
               'zone': 'one',
			   'pattern': [
                   {
                       "type": "ti_predefined_doubleclick_100"
                   }
               ]
            }
        ]
    }
    requests.post(f'http://{gamesense_url}/bind_game_event', json=clock_handler)

def send_time(time):
    event_data = {
        'game': app,
        'event': time_event,
        'data': { 'value': time }
    }
    requests.post(f'http://{gamesense_url}/game_event', json=event_data)

def send_bzzz(id):
    event_data = {
        'game': app,
        'event': bzzz_event,
        'data': { 'value': id }
    }
    requests.post(f'http://{gamesense_url}/game_event', json=event_data)

register_clock()
bind_clock_event()
bind_bzzz_event()


def show_time():
    while True:
        now = datetime.datetime.now()
        print(f"{now.hour}:{now.minute}:{now.second}")
        send_time(now.strftime("%X"))
        if not now.minute and not now.second:
            send_bzzz(now.hour)
        sleep(0.1)

show_time()