# HS System

- Create a new mariadb database
- run create sql scripts
- run the project


### Running via Docker
`docker run --detach --name some-mariadb -p 3306:3306 --env MARIADB_ROOT_PASSWORD=admin  mariadb:latest      ` 


### how to run load testing
- first create network
- connect: `docker network connect network hs `
- run `docker run --network network --rm -i grafana/k6 run - <./loadtesting.js`


#### Results
falco + donald 800-5000
█ TOTAL RESULTS 

    checks_total.......................: 126062  1660.010977/s
    checks_succeeded...................: 100.00% 126062 out of 126062
    checks_failed......................: 0.00%   0 out of 126062

    ✓ status was 200

    HTTP
    http_req_duration.......................................................: avg=4.98ms min=1ms med=3.7ms max=47.07ms p(90)=9.29ms p(95)=10.56ms
      { expected_response:true }............................................: avg=4.98ms min=1ms med=3.7ms max=47.07ms p(90)=9.29ms p(95)=10.56ms
    http_req_failed.........................................................: 0.00%  0 out of 126062
    http_reqs...............................................................: 126062 1660.010977/s

    EXECUTION
    iteration_duration......................................................: avg=1s     min=1s  med=1s    max=1.04s   p(90)=1s     p(95)=1.01s  
    iterations..............................................................: 126062 1660.010977/s
    vus.....................................................................: 3      min=3           max=4986
    vus_max.................................................................: 5000   min=5000        max=5000

    NETWORK
    data_received...........................................................: 51 MB  670 kB/s
    data_sent...............................................................: 28 MB  365 kB/s
