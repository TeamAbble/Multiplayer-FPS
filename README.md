# Multiplayer FPS
Created with Unity Gaming Services' Relay & Lobby
<br>Netcode written using FirstGearGames' FishNet.
<br>Using FGG's FishyUnityTransport for interoperability between Relay and FishNet.

## Index




## So what exactly is this project?
_That's a great question, reader!_<br>
This project is designed to be another venture into Netcode and Unity Gaming Services, with the intention being a small Peer-to-Peer hosted multiplayer first person shooter.
<br>
The project is uploaded to the team repository, but will primarily be worked on by Lunar-Skie Hackett (Github username LunarHackett003, profile link @ https://github.com/LunarHackett003).
<br>

## What does it do currently then, hotshot?
Another great question! You're full of these, aren't you?<br>
Currently, this is a rather hollow project with a minimalistic flow<br>
It has an authentication screen which takes you to a Unity Player Account login page (which is different from your Unity Developer Account). <br>
This then returns you to the game and finishes logging you in, saving your credentials.<br>
You load into the main menu, which has three big buttons:
* The first button lets you create a lobby.
  * This calls back to my `Connection Manager` script. This script then...
    * Creates a Relay Allocation
    * Connects the player to the allocation
    * Selects a scene at random from a list (scene selection will come later)
    * Creates a Lobby for the players, which is how games are discovered later on
* The second button connects you to a random lobby.
  * Back to the `Connection Manager` we go!
    * Rather than using the built-in `QuickJoinLobby` method that the Unity Lobby package provides, this uses custom logic. Quite simply, it picks a random number from all the lobby entries in the lobby list,
      grabs the join code from that entry and then calls the method on the `Connection Manager` to `JoinGameLobbyViaCode`
    * The player connects to the lobby FIRST, in contrast to players creating a lobby
    * The player then pulls the Relay Join Code from the Lobby's Data
    * The FishNet Network Manager is then given all the appropriate data to join the Relay Allocation
    * The player transitions to the gameplay scene after a short loading screen.
* The third button opens the Lobby List, which lets the player select a lobby.
  * The menu opens and the player will see a list of all available lobbies, a greyed-out button (this button is interactable when a lobby is selected), and a cancel button, which closes the menu.
  * The connection flow is mostly the same as connecting to a random lobby, but instead of finding a random one, it uses the information from the lobby the player selected.
  * The lobby list is refreshed every 30 seconds, but this is configurable as soon as I look up the rate limit on Lobby queries.

## Okay, but what happens next?
More questions? Okay, this one is cool! Maybe _now_ you'll be satisfied<br>
Once the player has joined a game, currently, all they can do is view the countdown of the Pre-Game Timer, followed by the countdown of the In-Game Timer.<br>
This, however, confirms that the gameplay states are somewhat functional.<br>

## So what are you planning next? 
Next, I plan on making the player able to possess a flying spectator cam when they are either dead, or when the game is not in progress. When the game IS in progress, or the player is alive, they should possess their player body.<br>
The player will call to a SpawnManager in the scene, which will allocate spawn points to each player, spawning the player in a radius around that spawnpoint.<br>
If the target spawn position is occupied, then the system will find a new spawn position.

## What about maps?
We have... One map so far. And its a square with some walls. Because uh... There's no player interaction yet. I haven't even tested if connecting to others works :P
![image](https://github.com/TeamAbble/Multiplayer-FPS/assets/61294652/5521b12f-336d-44c7-bb46-c0d8938bba07)
