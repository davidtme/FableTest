module RabbitQueue
open System

type Message1 = 
    { Body : string }

type QueueSettings = 
    { UserName : string
      Password : string
      HostName : string
      ExchangeName : string }

#if FABLE

open Fable.Core
open Fable.Import

[<Import("*", "amqplib/callback_api")>]
let private AmqpLib : obj = failwith "JS only"

let private channelClosed connection error = 
    connection?close $ () |> ignore
    ignore()

let private channelCreated (settings : QueueSettings, queueName : string, message) connection error channel = 
    let messageBuffer = createNew Node.Buffer $ (JS.JSON.stringify (message))
    let options = createObj [ "durable" ==> true ]
    channel?assertQueue $ (queueName, options) |> ignore
    channel?sendToQueue $ (queueName, messageBuffer) |> ignore
    channel?close $ (Func<_, _>(channelClosed connection)) |> ignore

let private connected a err connection = 
    connection?createChannel $ (Func<_, _, _>(channelCreated a connection)) |> ignore

let publishMessageToQueue settings message queueName = 
    AmqpLib?connect $ ("amqp://" + settings.HostName, Func<_, _, _>(connected (settings, queueName, message))) |> ignore

let subscribeToQueue queueName settings (handler : 'a -> unit) = 
    ignore()

#else

open RabbitMQ.Client
open Newtonsoft.Json
open System.Text
open System.Threading

let private createConnection (settings : QueueSettings) = 
    let factory = ConnectionFactory()
    factory.UserName <- settings.UserName
    factory.Password <- settings.Password
    factory.HostName <- settings.HostName
    factory.CreateConnection()

let private createModel (connection : IConnection) (settings : QueueSettings) queueName = 
    let model = connection.CreateModel()
    model.ExchangeDeclare(settings.ExchangeName, "direct", true)
    let a = model.QueueDeclare(queueName, true, false, false, null)
    model.QueueBind(queueName, settings.ExchangeName, queueName)
    model

let subscribeToQueue queueName settings (handler : 'a -> unit) = 
    let cts = new CancellationTokenSource()
    let wh = new AutoResetEvent(false)
    async { 
        use connection = createConnection settings
        use model = createModel connection settings queueName 
        model.BasicQos(0ul, 1us, false)
        let consumer = QueueingBasicConsumer(model)
        let a = model.BasicConsume(queueName, false, consumer)
        let timeout = TimeSpan.FromSeconds(10.).TotalMilliseconds |> int
        while not cts.IsCancellationRequested do
            match consumer.Queue.Dequeue(timeout) with
            | true, deliveryArgs -> 
                let json = Encoding.Default.GetString(deliveryArgs.Body)
                let message = JsonConvert.DeserializeObject<'a>(json)
                handler message
                model.BasicAck(deliveryArgs.DeliveryTag, false)
            | _ -> ignore()
        wh.Set() |> ignore
    }
    |> Async.Start
    fun () -> 
        cts.Cancel()
        cts.Dispose()
        wh.WaitOne() |> ignore


let publishMessageToQueue settings message queueName =
    use connection = createConnection settings
    use model = createModel connection settings queueName
    let properties = model.CreateBasicProperties()
    properties.Persistent <- true
    let json = JsonConvert.SerializeObject(message)
    let bytes = Encoding.Default.GetBytes(json)
    model.BasicPublish(settings.ExchangeName, queueName, properties, bytes)

#endif

let inline subscribe settings (handler : 'a -> unit) = 
    subscribeToQueue typeof<'a>.FullName settings handler

let inline publishMessage settings (message: 'a) = 
    publishMessageToQueue settings message typeof<'a>.FullName 
