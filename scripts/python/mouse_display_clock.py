"""
This script is designed to work on PCs with SteelSeries Engine installed.
It uses the GameSense API to display a clock and trigger events on SteelSeries devices.
Ensure the SteelSeries Engine is installed and running.
"""

from time import sleep
import datetime
from gamesense_utils import register_game, bind_event, create_event_data, send_event

# Constants
APP = 'CLOCK'
DISPLAY_NAME = 'Clock'
TIME_EVENT = 'TIME'
BZZZ_EVENT = 'BZZZ'

# Register the game
register_game(APP, DISPLAY_NAME)

# Bind the clock event
bind_event(APP, TIME_EVENT, [
    {
        'device-type': 'screened',
        'mode': 'screen',
        'zone': 'one',
        'datas': [
            {
                'icon-id': 15,
                'has-text': True,
                'length-millis': 1100
            }
        ]
    }
])

# Bind the vibration event
bind_event(APP, BZZZ_EVENT, [
    {
        'device-type': 'tactile',
        'zone': 'one',
        'mode': 'vibrate',
        'pattern': [
            {"type": "ti_predefined_strongclick_100", "delay-ms": 250},
            {"type": "ti_predefined_doubleclick_100", "delay-ms": 400},
            {"type": "ti_predefined_strongclick_100", "delay-ms": 500},
            {"type": "ti_predefined_doubleclick_100"},
        ]
    }
])

def send_time_event(time):
    """Send the current time as an event."""
    event_data = create_event_data(APP, TIME_EVENT, {'value': time})
    send_event(event_data)

def send_bzzz_event(id):
    """Send a vibration event."""
    event_data = create_event_data(APP, BZZZ_EVENT, {'value': id})
    send_event(event_data)

def show_time():
    """Continuously display the time and trigger events."""
    while True:
        now = datetime.datetime.now()
        print(f"{now.hour}:{now.minute}:{now.second}")
        send_time_event(now.strftime("%X"))
        if now.second == 0:
            send_bzzz_event(now.minute)  # Only triggers when now.hour == now.minute
        sleep(0.5)

# Start the clock display
show_time()