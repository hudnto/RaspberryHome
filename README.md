RaspberryHome
=============

A home automation system written in C# (so it can be run in a raspberry pi in mono).

You can control n-devices so whenever they are all not at home the webcam alarm is turned on and the philips hue lights are turned off. I was achieving this with Tasker in Android but when there is more than 1 person (i.e. more than 1 mobile device) then all this automation cannot be achieved with Tasker...

Technologies:

 - Akka.net
 - NancyFX

3rd party:

 - Pushbullet for mobile notifications
 - Philips Hue integration (lights on / off)
 - Webcam Alarm on / off

The App.Config needs to be edited beforehand otherwise the application won't work!
