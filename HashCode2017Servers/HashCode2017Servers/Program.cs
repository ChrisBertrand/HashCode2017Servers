using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HashCode2017Servers.Reader;

namespace HashCode2017Servers
{
    class Program
    {
        static void Main(string[] args)
        {
            var solver = new Solver();
            solver.Solve();
        }
    }

    class Solver
    {
        public struct VidCosting
        {
            public int id;
            public int vidId;
            public int size;
            public int cacheId;
            public int latency;
            public int latencySave;
        }

        public struct CacheFormation
        {
            public Cache c;
            public List<int> videos;
        }

        public void Solve()
        {
            var reader = new Reader();
            reader.Read();

            // Solve the problem.
            List<VidCosting> VidCost = new List<VidCosting>();

                // Need to merge the requests
                //var CombinedRequests = reader.Requests.GroupBy(f => f.vId).Select(req => req).ToList();
                var CombinedRequests = reader.Requests;

            CacheFormation result = new CacheFormation();
            result.videos = new List<int>();

            // Get the lowest latency, find cost for each request
            foreach (var req in CombinedRequests)
            {
                int lD = reader.Endpoints.Where(en => req.sourceEndPoint == en.id).Select(an => an.latencyDataCenter).First();
                foreach (EndpointCacheLatency c in reader.Endpoints.Where(en => en.id == req.sourceEndPoint).Select(enr => enr.cacheLatency).First())
                {
                    int latencySave = lD - c.latency;
                    // try adding if not too big.
                    VidCost.Add(new VidCosting {id= req.id, cacheId = c.cacheid, latency = c.latency, latencySave = latencySave, vidId=req.vId,
                        size = reader.Videos.Where(v => v.id == req.vId).First().size });
                }

                // Which cache are we on?
                var cach = reader.Caches.Where(ca => ca.id == VidCost.First().cacheId).First();

                // sort my best latency save
                var vidsToFit = VidCost.Where(z=> z.cacheId == cach.id).OrderByDescending(a => a.latencySave).TakeWhile(b => b.size < cach.capacity);
                
                // Remove capacity.
                cach.capacity = -VidCost.First().size;
            }
            var where = 1;
        }

        public double Cost()
        {
            return int.MaxValue;
        }
    }

        class Reader
    { 
        public List<Video> Videos;
        public struct Video
        {
            public int id;
            public int size;
        }

        public List<Endpoint> Endpoints;
        public struct Endpoint
        {
            public int id;
            public int latencyDataCenter;
            public bool[] connected;
            public List<EndpointCacheLatency> cacheLatency;
        }

        public struct EndpointCacheLatency
        {
            public int id;
            public int cacheid;
            public int latency;
        }

        public List<Cache> Caches;
        public struct Cache
        {
            public int id;
            public int capacity;
        }

        public List<Request> Requests;
        public struct Request
        {
            public int id;
            public int requests;
            public int vId;
            public int sourceEndPoint;
        }

        public int noOfVideos;
        public int noOfEp;
        public int noOfRequests;
        public int noOfCaches;
        public int cacheSize;

        public void Read()

        {
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        var FileDetails = File.ReadAllLines("me_at_the_zoo.in");

            string[] firstline = FileDetails.First().Split(' ');

            noOfVideos = Convert.ToInt32(firstline[0]);
            noOfEp = Convert.ToInt32(firstline[1]);
            noOfRequests = Convert.ToInt32(firstline[2]);
            noOfCaches = Convert.ToInt32(firstline[3]);
            cacheSize = Convert.ToInt32(firstline[4]);
            var CurrentFilePos = FileDetails.Skip(1);
            string[] videos = CurrentFilePos.First().Split(' ');
            Videos = new List<Video>();

            // create cache sizes
            Caches = new List<Cache>();
            for (int i=0; i<noOfCaches;i++)
            {
                Caches.Add(new Cache { id = i, capacity = cacheSize });
            }

            // Create videos with sizes
            for (int i=0; i< videos.Length; i++)
            {
                Video video = new Video() { id = i, size = Convert.ToInt32(videos[i]) };
                Videos.Add(video);
            }

            // Create endpoints
            Endpoints = new List<Endpoint>();

            // loop over endpoints and number of connected caches
            for (int i = 0; i < noOfEp; i++)
            {
                CurrentFilePos = CurrentFilePos.Skip(1);
                string[] endpoint = CurrentFilePos.First().Split(' ');
                Endpoint end = new Endpoint() { id = i, latencyDataCenter = Convert.ToInt32(endpoint[0])};
                end.cacheLatency = new List<EndpointCacheLatency>();
                var noOfCachesConnected = Convert.ToInt32(endpoint[1]);

                for (int j=0; j < noOfCachesConnected ; j++)
                {
                    CurrentFilePos = CurrentFilePos.Skip(1);
                    string[] endpointLatencyCache = CurrentFilePos.First().Split(' ');
                    var cacheLatencyToEndpoint = new EndpointCacheLatency { cacheid = Convert.ToInt32(endpointLatencyCache[0]), latency = Convert.ToInt32(endpointLatencyCache[1])};
                    end.cacheLatency.Add(cacheLatencyToEndpoint);
                }
                Endpoints.Add(end);
            }

            


            // loop over requests.
            Requests = new List<Request>();
            for (int req = 0; req< noOfRequests; req++)
            {
                CurrentFilePos = CurrentFilePos.Skip(1);
                string[] reqs = CurrentFilePos.First().Split(' ');
                Request r = new Request { id = req, vId = Convert.ToInt32(reqs[0]), sourceEndPoint = Convert.ToInt32(reqs[1]), requests = Convert.ToInt32(reqs[2]) };
                Requests.Add(r);
            } 

            var ret = true;
        }

    public void Write()
    {

    }
    }
}
