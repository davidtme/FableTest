module Program

open RabbitQueue

// Temp moved here:
let inline publishMessage settings (message: 'a) = 
    publishMessageToQueue settings message typeof<'a>.FullName 


[<EntryPoint>]
let main argv =

    let settings = 
        { UserName = "guest"
          Password = "guest"
          HostName = "localhost"
          ExchangeName = "Test1" }

    publishMessage settings { Body = "Hello" } 
    0
