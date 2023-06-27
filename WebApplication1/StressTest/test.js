import http from 'k6/http';
import { sleep } from 'k6';




export let options = {
    insecureSkipTLSVerify: true,
    noConnectionReuse: false,
    scenarios: {
        constant_request_rate: {
            executor: 'constant-arrival-rate',
            rate: 78, // Desired number of requests per second
            timeUnit: '1s', // Time unit for the rate
            duration: '10s',
            preAllocatedVUs: 1000, // Number of VUs to pre-allocate
            maxVUs: 1500, // Maximum number of VUs
        },
    },
};

const url = 'https://localhost:7264/api/Issue';

const defaultString = {
    "tenantId": "first",
    "metricType": "first",
    "metricValue": 0,
    "jsonField": "first"
};

const payload = [
];

for (var i = 0; i < 100; i++) {
    payload.push(defaultString);
};




const headers = {
    "Content-Type": "application/json",
};

export default () => {
    http.post(url, JSON.stringify(payload), { headers: headers });
    sleep(1);
};

