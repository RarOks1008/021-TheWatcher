# The Watcher

*C# Console Application for Watching changes on Websites made using Selenium.*


## Application Design

### *Console View*
![Console View](git-image/image1.png)

View of the console while the application is running.

### *Email View*
![Email View](git-image/image2.png)

Email notification of errors.

## Other remarks

### Files

- You only need to add the **watcher.json** file in the *bin/Debug* folder
- **watcher.json** file should be made according to the example given bellow:
```
{
	"Urls": [
		{
			"Title": "Title",
			"Url": "https://someurl.com",
			"Value": "Value1",
			"XPath": "FullXPath"
		}
	],
	"EmailSender": {
		"ToEmail" : "email@email.com",
		"Email": "email@email.com",
		"Password": "Pass123"
	},
	"CheckDuration": 3600000,
	"ShouldShowSuccess": false
}
```
- properties in the json file are:
  - *CheckDuration* - time in milliseconds for when to run the next set of checks. (3600000=1hour)
  - *EmailSender* - Object which holds data for sending the email.
  - *ToEmail* - email on which to send notification.
  - *Email* and *Password* - email and password for an email from which the notification will be sent.
  - *Urls* - array of urls to check.
  - *Title* - title of your url to check.
  - *Url* - url to navigate to.
  - *Value* - value expected to get on the element.
  - *XPath* - FullXPath of the element to check.
  - *ShouldShowSuccess* - boolean value to check if the success values should be written in the console.
  - *NotificationType* - either console or email to check the type of sending the notification.
