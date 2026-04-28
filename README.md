# AGI Decrypter

                ©2026
           by Andrew Korson
    
    ==============================                                   
              Version 3
    ==============================

A modern port of the original AGI Decrypter, originally written in VB6.

Allows a user to select a folder containing Sierra's AGI game files and decrypts the interpreter. It will also modify the loader program (usually a .COM file using the game's ID as the name, or sometimes 'SIERRA.COM') by embedding the key and bypassing the original disk check. Specifically:
 * the encrypted file AGI is decrypted and saved as AGI.EXE
 * the loader file SIERRA.COM (or the .COM file with the game's ID as the name) is modified to bypass the disk check and embed the decryption key, allowing it to run without the original game files.
 * the decryption key is stored in a text file named "key.txt" for reference.

NOTE: The original disks are NOT needed for decryption; the file AGI is sufficient. The tool is able to reconstruct the decryption key from the AGI file alone.

### Licensing

    Copyright (C) 2005 - 2026 Andrew Korson

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>. 
