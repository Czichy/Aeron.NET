﻿using System;
using System.Text;
using System.Threading;
using Adaptive.Agrona.Concurrent;

namespace Adaptive.Aeron.Samples.SimplePublisher
{
    /// <summary>
    /// A very simple Aeron publisher application
    /// Publishes a fixed size message on a fixed channel and stream. Upon completion
    /// of message send, it lingers for 5 seconds before exiting.
    /// </summary>
    public class SimplePublisher
    {
        public static void Main()
        {
            // Allocate enough buffer size to hold maximum message length
            // The UnsafeBuffer class is part of the Agrona library and is used for efficient buffer management
            var buffer = new UnsafeBuffer(new byte[512]);

            // The channel (an endpoint identifier) to send the message to
            const string channel = "udp://localhost:40123";

            // A unique identifier for a stream within a channel. Stream ID 0 is reserved
            // for internal use and should not be used by applications.
            const int streamId = 10;

            Console.WriteLine("Publishing to " + channel + " on stream Id " + streamId);

            // Create a context, needed for client connection to media driver
            // A separate media driver process needs to be running prior to starting this application
            var ctx = new Aeron.Context();

            // Create an Aeron instance with client-provided context configuration and connect to the
            // media driver, and create a Publication.  The Aeron and Publication classes implement
            // AutoCloseable, and will automatically clean up resources when this try block is finished.
            using (var aeron = Aeron.Connect(ctx))
            using (var publication = aeron.AddPublication(channel, streamId))
            {
                Thread.Sleep(100);

                const string message = "Hello World! ";
                var messageBytes = Encoding.UTF8.GetBytes(message);
                buffer.PutBytes(0, messageBytes);

                // Try to publish the buffer. 'offer' is a non-blocking call.
                // If it returns less than 0, the message was not sent, and the offer should be retried.
                var result = publication.Offer(buffer, 0, messageBytes.Length);

                if (result < 0L)
                {
                    switch (result)
                    {
                        case Publication.BACK_PRESSURED:
                            Console.WriteLine(" Offer failed due to back pressure");
                            break;
                        case Publication.NOT_CONNECTED:
                            Console.WriteLine(" Offer failed because publisher is not yet connected to subscriber");
                            break;
                        case Publication.ADMIN_ACTION:
                            Console.WriteLine("Offer failed because of an administration action in the system");
                            break;
                        case Publication.CLOSED:
                            Console.WriteLine("Offer failed publication is closed");
                            break;
                        default:
                            Console.WriteLine(" Offer failed due to unknown reason");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine(" yay !!");
                }

                Console.WriteLine("Done sending.");
                Console.WriteLine("Press any key...");
                Console.ReadLine();
            }
        }
    }
}