import json
import requests

# Load the GameSense URL from the coreProps.json file
corePropsPath = '/ProgramData/SteelSeries/SteelSeries Engine 3/coreProps.json'
with open(corePropsPath, 'r') as file:
    gamesense_url = json.load(file)['address']

def register_game(app, display_name):
    """Register a game with GameSense."""
    metadata = {
        'game': app,
        'game_display_name': display_name,
    }
    requests.post(f'http://{gamesense_url}/game_metadata', json=metadata)

def bind_event(app, event, handlers):
    """Bind an event to GameSense."""
    event_data = {
        'game': app,
        'event': event,
        'handlers': handlers
    }
    requests.post(f'http://{gamesense_url}/bind_game_event', json=event_data)

def create_event_data(app, event, data):
    """Create event data for sending to GameSense."""
    return {
        'game': app,
        'event': event,
        'data': data
    }

def send_event(event_data):
    """Send an event to GameSense."""
    requests.post(f'http://{gamesense_url}/game_event', json=event_data)