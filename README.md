# Visual-Cryptography
A Windows Application based on the principle of randomization of pixels to encrypt and decrypt colored image using a reversible algorithm and further splitting it into shares to transmit the image securely over an unreliable network
## Synopsis
1. Designed a windows application based on the principle of randomization of pixels to encrypt and decrypt colored image using a reversible algorithm and further splitting it into four shares to transmit the image securely over an unreliable network using C#.NET 
2. Implemented additional feature like displaying user entered text on the encrypted image to provide two level of security by deceiving the hacker
## How to Setup
### Prerequisites :
Visual Studio 2008/Visual Studio 2010, C#
### Steps :
1. For login details to login into the application refer the username & password text file
2. Enter the text in the textbox (max 10 characters) that you want to display on the encrypted image
2. Click on encrypt button, now select the image you want to encrypt<br>
(Encrypted files will be stored to disk where original image is present)
3. Now to decrypt please re-enter the same text in textbox
4. Click on merge button, select files (4 encrypted files i.e. four shares created after encryption of the original image)
5. The decrypted image will be saved to your disk<br>
(Same place where encrypted files are present)


