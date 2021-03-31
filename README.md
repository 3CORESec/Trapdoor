<p align="center"><img src="./imgs/logo/trapdoor-small.png" width="600" height="380"></p>

# Trapdoor - A serverless honeytoken framework

Trapdoor is an AWS Serverless Application meant to create and alert on [honeyTokens](https://community.broadcom.com/symantecenterprise/communities/community-home/librarydocuments/viewdocument?DocumentKey=74450cf5-2f11-48c5-8d92-4687f5978988&CommunityKey=1ecf5f55-9545-44d6-b0f4-4e4a7f5f5e68&tab=librarydocuments).

<img align="right" src="./imgs/screenshot.png" width="440" height="249">

- Slack notifications
- Webhook notifications
- Aggregation (by IP and session) & client tracking
- HTTP-based and JavaScript-based fingerprinting
- Tracking and editing of Slack messages to enrich existing alerts
- Custom paths and friendly reminders
- & more!

Trapdoor is inspired by the awesome work of [Adel](https://twitter.com/0x4d31) in [honeyLambda](https://github.com/0x4D31/honeyLambda).

We'll provide updates on new features and bug fixes in our blog. Visit the following articles to know more:

- [Trapdoor Announcement - The serverless HTTP Honeypot](https://blog.3coresec.com/2021/03/trapdoor-serverless-http-honeypot.html)
- [3CORESec Blog - Trapdoor-tagged posts](https://blog.3coresec.com/search/label/Trapdoor)

## Installation

Trapdoor is available as a serverless application on AWS Serverless Application Repository. In the region where you'd like to deploy Trapdoor, visit the Serverless Application Repository _(in AWS Console, just search for it in the Services section)_ and head over to Available Applications.

Search for Trapdoor _(make sure to enable "show apps that create custom IAM roles or resources policies")_ and click on deploy. 

While the installation of Trapdoor is fully automatic you will have to provide some input to the application before it can be deployed to your account depending on which alert modules you'd like to enable. Please check the **Alert** section below before continuing.

## Alert Configuration

Trapdoor provides 2 alert mechanisms:

- HTTP POST / Webhook
- Slack Notifications

You can enable one of them or both. Enabling the alert method requires only that you enter the information in the deployment page of AWS Serverless Application Repository, as we'll explain below.

### HTTP POST

To enable the HTTP POST option *(where Trapdoor will send a JSON structure of its findings to the specificed URL)* simply paste the URL in the `POSTURL` variable.

### Slack

Trapdoor also allows you to have notifications and alerts sent to a Slack channel. This section will provide you with detailed information on how to create an app/bot to send your Trapdoor notifications.

1. Visit the [Apps section](https://api.slack.com/apps) on Slack and click on **Create New App**
2. Give it a name and choose the desired Slack
3. Visit the **OAuth & Permissions** section of the app and, under **Scopes - Bot Token Scopes**, "Add a OAuth Scope" for `chat:write`
4. At the top of the screen click on **Install to Workspace** and make note of `Bot User OAuth Token`
5. Invite the bot to the channel for which you'd like to have the messages posted to _(simply typing @bot_name will allow you to do so)_

Additionally you'll also require the ID of the channel that Trapdoor will be sending messages to. You can retrieve this information by visiting the channel in Slack Web, as demonstrated in the image below:

<p align="center"><img src="./imgs/slack-channel-id.png" width="680" height="206"></p>

You now have all the information required to deploy via the AWS Serverless Application Repository.

- **SLACKPATH:** `https://your_team.slack.com` _(example: https://3coresec.slack.com)_
- **WEBHOOKCHANNEL:** ID that was retrieved via Slack Web _(example: `C0114EEEG59`)_
- **WEBHOOKTOKEN:** `Bot User OAuth Token` from the previously created app

## Trapdoor Setup

After the deployment is complete you can create your tokens by editing the `config.json` _(in the AWS Lambda page)_ and adding both a path as well as a friendly reminder:

```
...
{
  "Paths": {
    "admin": "Token present in honeypot in Germany",
    "ftp": "Token from .txt in Raspberry"
  },
...
```

### Domains & Customization

Consider using your custom domains instead of the AWS API URLs _(and map them to the /Prod stage in AWS API)_ so that your tokens can be made available under, for example, `https://important-corp.com/login`. Bear in mind that you can associate unlimited *(different)* domains to an API in AWS API GW, so it's really up to you to configure the best deception options for your tokens 🕵🏻‍♂️

## Usage

Using Trapdoor is as simple as visiting the API Endpoint that is made available in the Lambda Application dashboard *(presented after the deployment is complete)*:

<p align="center"><img src="./imgs/api-screen.png" width="319" height="231"></p>

While all paths *($API/Prod/WHATEVER)* are accepted and alerted, choosing a path that is configured in Trapdoor `config.json` will provide you with a friendly reminder of where that token is located/stored.

## Feedback

Found this interesting? Have a question/comment/request? Let us know!

Feel free to open an [issue](https://github.com/3CORESec/Trapdoor/issues) or ping us on [Twitter](https://twitter.com/3CORESec). We also have a [Community Slack](https://launchpass.com/3coresec) where you can discuss our open-source projects, participate in giveaways and have access to projects before they are released to the public.

[![Twitter](https://img.shields.io/twitter/follow/3CORESec.svg?style=social&label=Follow)](https://twitter.com/3CORESec)

## Legal notice

3CORESec is releasing this project as a proof-of-concept for the research community.

Please remember that it might not be legal to run Trapdoor in some countries and that the information you will be accessing could be considered personal data.

If you decide to deploy, install or run Trapdoor you will be agreeing to release and hold us harmless from any responsibility resulting or arising directly or indirectly from the use of Trapdoor.

You are solely and exclusively responsible for the use of Trapdoor.
