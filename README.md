# Pub Sub

This is an example on how to achieve a pub sub pattern with the help of [Channels](https://github.com/goncalo-oliveira/channels) and *Parcel* protocol.

This solution offers no persistence or delivery guarantees. Only connected clients will receive messages. Essentially, the broker will keep track of the clients that are subscribed to a given topic and will forward messages to them.

## Broker

The broker is a simple server that will accept connections from clients and forward messages to them. Using a channel service, it will keep track of connected clients.

when a new message is published, the broker will forward it to all clients that are subscribed to the topic.

## Client

A client is a subscriber or a publisher (or both). Upon connnecting, it send subscribe and/or publish messages to the broker.

Messages sent to the broker need to follow the *Parcel* protocol. Furthermore, there the identifier of the message must be a topic instruction to either subscribe, publish or unsubscribe. The format of the identifier should be `instruction:topic`, where `instruction` is one of `subscribe`, `publish` or `unsubscribe` and `topic` is the topic to subscribe to, publish to or unsubscribe from.


## Subscribing to a topic

Upon connecting, the client can subscribe to one or more topics. The broker will keep track of the client's subscriptions and will forward messages to the client when a message is published to a topic that the client is subscribed to. To subscribe to a topic, the client must send a message with the `subscribe:topic` identifier, where `topic` is the topic to subscribe to. Any of the aliases for the subscribe instruction can be used, namely `subscribe`, `sub` or `+`. The topic can only contain alphanumeric characters, dashes and underscores.

```json
{
  "id": "subscribe:topic_name",
}
```

## Unsubscribing from a topic

To unsubscribe from a topic, the client must send a message with the `unsubscribe:topic` identifier, where `topic` is the topic to unsubscribe from. Any of the aliases for the unsubscribe instruction can be used, namely `unsubscribe`, `unsub` or `-`. The topic can only contain alphanumeric characters, dashes and underscores.

```json
{
  "id": "unsubscribe:topic_name",
}
```

## Publishing a message

To publish a message to a topic, the client must send a message with the `publish:topic` identifier, where `topic` is the topic to publish to. Any of the aliases for the publish instruction can be used, namely `publish`, `pub` or an empty string (`:topic`). The topic can only contain alphanumeric characters, dashes and underscores.

```json
{
  "id": ":topic_name",
  "message": "Hello World!"
}
```
