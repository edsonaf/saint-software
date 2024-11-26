# Given parameter A (in minutes) a countdown is started
# When the clock reaches zero, given parameter B is displayed on the mouse
# And the mouse will start buzzing

import argparse
import json
import requests
from time import sleep
import datetime

app = 'TIMER'
display_name = 'Timer'
time_event = 'TIME_REMAINING'
bzzz_event = 'BZZZ_END'

parser = argparse.ArgumentParser(description='Description of your script')
parser.add_argument('count_down', help='value (in minutes) as initial countdown')
args = parser.parse_args()

corePropsPath = '/ProgramData/SteelSeries/SteelSeries Engine 3/coreProps.json'
gamesense_url = json.load(open(corePropsPath))['address']

def register_clock():
    timer_metadata = {
        'game': app,
        'game_display_name': display_name,
    }
    r = requests.post('http://'+gamesense_url+'/game_metadata', json=timer_metadata)

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
    r = requests.post('http://'+gamesense_url+'/bind_game_event', json=clock_handler)

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
    r = requests.post('http://'+gamesense_url+'/bind_game_event', json=clock_handler)

def send_time(time):
    event_data = {
        'game': app,
        'event': time_event,
        'data': { 'value': time }
    }
    r = requests.post('http://'+gamesense_url+'/game_event', json=event_data)

def send_bzzz(id):
    event_data = {
        'game': app,
        'event': bzzz_event,
        'data': { 'value': id }
    }
    r = requests.post('http://'+gamesense_url+'/game_event', json=event_data)

register_clock()
bind_clock_event()
bind_bzzz_event()

end_time = datetime.datetime.now() + datetime.timedelta(minutes=int(args.count_down))
while datetime.datetime.now() < end_time:
    remaining_time = end_time - datetime.datetime.now()
    total_seconds = int(remaining_time.total_seconds())

    # Calculate hours, minutes, and seconds
    hours, remainder = divmod(total_seconds, 3600)
    minutes, seconds = divmod(remainder, 60)

    # Format into "HH:MM:SS" string
    time_str = f"{hours:02}:{minutes:02}:{seconds:02}"


    print(f"Time remaining: {remaining_time}")
    send_time(time_str)
    sleep(1)

# we reached zero
send_bzzz(0)
# print("Countdown reached zero. Exiting.")