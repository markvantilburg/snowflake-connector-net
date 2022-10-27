﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Snowflake.Data.Client;

namespace Snowflake.Data.Core
{
    public sealed class HeartBeatBackground
    {
        private static HeartBeatBackground instance = null;
        private static readonly object heartBeatLock = new object();

        private static readonly object _heartBeatLock = new object();
        private static List<SFSession> heartBeatConns = null;

        private static Thread heartBeatThread = null;
        private static long masterTokenValidationTimeInSec = 0;
        private static bool isHeartBeatEnd = false;

        private HeartBeatBackground()
        {

        }

        ~HeartBeatBackground()
        {
            isHeartBeatEnd = true;
        }

        public static HeartBeatBackground Instance
        {
            get
            {
                lock (heartBeatLock)
                {
                    if(instance == null)
                    {
                        instance = new HeartBeatBackground();
                    }
                    return instance;
                }
            }
        }

        private long getHeartBeatInterval(long masterTokenValidationTime)
        {
            return masterTokenValidationTime / 4;
        }

        public void addConnection(SFSession conn, long masterTokenValidityInSecs)
        {
            lock(heartBeatLock)
            {
                if (heartBeatConns == null)
                {
                    heartBeatConns = new List<SFSession>();
                }

                heartBeatConns.Add(conn);
                if(heartBeatThread == null)
                {
                    masterTokenValidationTimeInSec = masterTokenValidityInSecs;
                    heartBeatThread = new Thread(heartBeatAll);
                    heartBeatThread.Start();
                }
                else
                {
                    if(masterTokenValidityInSecs < masterTokenValidationTimeInSec)
                    {
                        //update validation time
                        masterTokenValidationTimeInSec = masterTokenValidityInSecs;
                    }
                }
            }
        }

        public void removeConnection(SFSession conn)
        {
            lock(heartBeatLock)
            {
                heartBeatConns.Remove(conn);
            }
        }

        void heartBeatAll()
        {
            while (true)
            {
                List<SFSession> copyOfHeartBeatQueue = new List<SFSession>();
                
                long heartBeatInterval = getHeartBeatInterval(masterTokenValidationTimeInSec);
                Thread.Sleep(TimeSpan.FromSeconds(heartBeatInterval));

                if (isHeartBeatEnd)
                {
                    return;
                }
                else
                {
                    lock (heartBeatLock)
                    {
                        copyOfHeartBeatQueue.AddRange(heartBeatConns);
                    }
                }

                for(int i = 0; i < copyOfHeartBeatQueue.Count(); i++)
                {
                    copyOfHeartBeatQueue[i].heartbeat();
                }
            }
        }
    }
}
