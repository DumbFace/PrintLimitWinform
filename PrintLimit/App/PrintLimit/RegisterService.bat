@echo off
sc create PrintLimit binPath= "C:\Program Files (x86)\VinaAi\PrintManager\PrintLimit.exe" start= auto
sc start PrintLimit