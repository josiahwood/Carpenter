# Carpenter

Carpenter is a generative AI interface built on top of [AI Horde](https://aihorde.net/).

# Features

## Model Rating
Each time a response is generated, two alternatives are presented from different models. When a winner is selected, the losing response is deleted and the ratings of each model are updated using the Elo rating system (like chess). These ratings are user specific and do not impact other users. Models with higher ratings are more likely to be used for that user.

## Recursive Summarization
The "Chat Summarization Prompt" page allows you to set a prompt that will be used to recursively summarize the chat history so that relevant details remain in the limited prompt size.