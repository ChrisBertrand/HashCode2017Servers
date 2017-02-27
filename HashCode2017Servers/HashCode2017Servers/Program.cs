using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HashCode2017Servers.Endpoint;
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
        public struct RequestCosting
        {
            public int id;
            public int noOfReqs;
            public Video video;
            public int cacheId;
            public int latency;
            public int latencySave;
        }

        public struct CacheFormation
        {
            public Cache ca;
            public List<int> videos;
        }

        public struct CacheResults
        {
            public List<CacheFormation> res;
        }

        public void Solve()
        {
            var reader = new Reader();
            reader.Read();

            // Solve the problem.
            List<RequestCosting> RequestCostings = new List<RequestCosting>();

            // Need to merge the requests
            var CombinedRequestsGroup = reader.requests.RequestList.GroupBy(f => f.vId);
            var CombinedRequests = CombinedRequestsGroup.SelectMany(req => req);
            //var CombinedRequests = reader.requests.RequestList;

            CacheFormation result = new CacheFormation();
            CacheResults results = new CacheResults();

            // Get the lowest latency, find cost for each request
            foreach (var req in CombinedRequests)
            {
                //Latency to DataCentre from Endpoint
                int lD = reader.endpoints.EndpointList.Where(en => req.sourceEndPoint == en.id).Select(an => an.latencyDataCenter).First();

                //Get all caches connected to endpoint
                foreach (ConnectedCaches c in reader.endpoints.EndpointList.Where(en => en.id == req.sourceEndPoint).Select(enr => enr.cacheLatency).First())
                {
                    int latencySave = lD - c.latency;

                    // try adding if not too big.
                    RequestCostings.Add(new RequestCosting { id = req.id, cacheId = c.cacheid, latency = c.latency, latencySave = latencySave, noOfReqs = req.requests,
                        video = reader.vids.getVideoById(req.vId) }
                     );
                }
            }

            // sort my best latency save
            foreach (Cache c in reader.caches.CacheList)
            {
                Console.WriteLine("Starting Cache: " + c.id);
                // Get the best videos for this request. Latency * NoOfRequests / VideoSize
                var bestVidsForCache = RequestCostings.Where(z => z.cacheId == c.id).OrderByDescending(a => (a.latencySave * a.noOfReqs) / a.video.size);

                //Create new cacheFormation for this cache
                if (results.res == null) { results.res = new List<CacheFormation>(); }
                result.videos = new List<int>();
                result.ca = reader.caches.getCacheById(c.id);

                for (int i=0; i<bestVidsForCache.Count(); i++)
                {
                    var rq = bestVidsForCache.Skip(i).First();
                    // Check we're not over capacity, and the next video wont push us over either.
                    if (c.capacity > 0 && (c.capacity - rq.video.size > 0))
                    {
                        // The video is not already added to this CacheFormation
                        if (result.videos.IndexOf(rq.video.id) == -1)
                        {
                            result.videos.Add(rq.video.id);
                            c.capacity = c.capacity - rq.video.size;
                        }
                    }
                }
                results.res.Add(result);
            }

            Write(results, reader);

            var where = 1;
        }

        private void Write(CacheResults results, Reader reader)
        {
            StreamWriter output = File.CreateText("output_" + reader.input  + ".txt");
            //First Line
            output.WriteLine(results.res.Count());

            foreach (CacheFormation cacheResults in results.res)
            {
                output.WriteLine(string.Join(" ", cacheResults.videos.ToArray()));
            }
            output.Close();
        }
    }

    class Videos
    {
        public List<Video> VideoList { get; set; }

        public Videos()
        {
            if (VideoList == null)
            {
                VideoList = new List<Video>();
            }
        }

        public Video getVideoById(int id)
        {
            return VideoList.Where(v => v.id == id).First();
        }
    }

    class Video
    {
        public int id { get; set; }
        public int size { get; set; }

        public Video(int id, int size)
        {
            this.id = id;
            this.size = size;
        }
    }

    class Endpoints
    {
        public List<Endpoint> EndpointList;

        public Endpoints()
        {
            if (EndpointList == null)
            {
                EndpointList = new List<Endpoint>();
            }
        }

        public Endpoint getEndpointById(int id)
        {
            return EndpointList.Where(v => v.id == id).First();
        }
    }

    class Endpoint
    {
        public int id { get; set; }
        public int latencyDataCenter { get; set; }
        public bool[] connected { get; set; }

        public List<ConnectedCaches> cacheLatency;

        public struct ConnectedCaches
        {
            public int cacheid;
            public int latency;
        }

        public Endpoint(int id, int latencyDataCenter)
        {
            this.id = id;
            this.latencyDataCenter = latencyDataCenter;

            if (cacheLatency == null)
            {
                cacheLatency = new List<ConnectedCaches>();
            }
        }
    }

    public class Caches
    {
        public List<Cache> CacheList;

        public Caches()
        {
            if (CacheList == null)
            {
                CacheList = new List<Cache>();
            }
        }

        public Cache getCacheById(int id)
        {
            return CacheList.Where(v => v.id == id).First();
        }

    }

    public class Cache
    {
        public int id { get; set; }
        public int capacity { get; set; }
        public Cache(int id, int capacity)
        {
            this.id = id;
            this.capacity = capacity;
        }
    }

    public class Requests
    {
    public List<Request> RequestList;

    public Requests()
    {
        if (RequestList == null)
        {
            RequestList = new List<Request>();
        }
    }
}

    public class Request
    {
        public int id;
        public int requests;
        public int vId;
        public int sourceEndPoint;
    }

    class Reader
    {
        public Videos vids = new Videos();
        public Endpoints endpoints = new Endpoints();
        public Caches caches = new Caches();
        public Requests requests = new Requests();

        public int noOfVideos;
        public int noOfEp;
        public int noOfRequests;
        public int noOfCaches;
        public int cacheSize;
        public string input;

        public void Read()
        {
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        input = "trending_today.in";
        var FileDetails = File.ReadAllLines(input);

        string[] firstline = FileDetails.First().Split(' ');

        noOfVideos = Convert.ToInt32(firstline[0]);
        noOfEp = Convert.ToInt32(firstline[1]);
        noOfRequests = Convert.ToInt32(firstline[2]);
        noOfCaches = Convert.ToInt32(firstline[3]);
        cacheSize = Convert.ToInt32(firstline[4]);
        var CurrentFilePos = FileDetails.Skip(1);
        string[] videoArray = CurrentFilePos.First().Split(' ');

        // create cache sizes
        for (int i=0; i<noOfCaches;i++)
        {
            caches.CacheList.Add(new Cache(i,cacheSize));
        }

        // Create videos with sizes
        for (int i=0; i< videoArray.Length; i++)
        {
            Video video = new Video(i, Convert.ToInt32(videoArray[i]));
            vids.VideoList.Add(video);
        }

        // loop over endpoints and number of connected caches
        for (int i = 0; i < noOfEp; i++)
        {
            CurrentFilePos = CurrentFilePos.Skip(1);
            string[] endpoint = CurrentFilePos.First().Split(' ');
            Endpoint end = new Endpoint(i, Convert.ToInt32(endpoint[0]));

            var noOfCachesConnected = Convert.ToInt32(endpoint[1]);

            for (int j=0; j < noOfCachesConnected ; j++)
            {
                CurrentFilePos = CurrentFilePos.Skip(1);
                string[] endpointLatencyCache = CurrentFilePos.First().Split(' ');

                    int cacheid = Convert.ToInt32(endpointLatencyCache[0]);
                    int EndLat = Convert.ToInt32(endpointLatencyCache[1]);
                    var cacheLatencyToEndpoint = new ConnectedCaches { cacheid = cacheid, latency = EndLat};
                    end.cacheLatency.Add(cacheLatencyToEndpoint);
            }
                endpoints.EndpointList.Add(end);
            }

            // loop over requests.
            for (int req = 0; req< noOfRequests; req++)
            {
                CurrentFilePos = CurrentFilePos.Skip(1);
                string[] reqs = CurrentFilePos.First().Split(' ');
                Request r = new Request { id = req, vId = Convert.ToInt32(reqs[0]), sourceEndPoint = Convert.ToInt32(reqs[1]), requests = Convert.ToInt32(reqs[2]) };
                requests.RequestList.Add(r);
            } 
            var ret = true;
            Console.WriteLine("File Read: " + stopWatch.Elapsed.ToString());
        }
    }
}
