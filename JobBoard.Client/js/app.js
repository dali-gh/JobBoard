const API_URL = "https://localhost:7131/graphql/";
const container = document.querySelector("#jobs");

async function loadJobs() {
    const response = await fetch(API_URL, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            query: `query {
                jobs(first: 4) {
                    edges {
                        cursor
                        node {
                            id
                            title
                            description
                            location
                            company {
                                id
                                email
                                location
                            }
                        }
                    }
                    pageInfo {
                        hasNextPage
                        endCursor
                    }
                }
            }`
        })
    });

    const result = await response.json();
    const connection = result.data.jobs;
    const jobs = connection.edges.map(edge => edge.node);

    container.innerHTML = jobs.map(job => `
        <div class="job">
            <h2>${job.title}</h2>
            <p>${job.description}</p>
            <p>Location: ${job.location}</p>
            <h3>Company</h3>
            <p>Email: ${job.company?.email ?? "Not provided"}</p>
            <p>Location: ${job.company?.location ?? "Not provided"}</p>
        </div>
    `).join("");

    console.log("Has next page:", connection.pageInfo.hasNextPage);
    console.log("End cursor:", connection.pageInfo.endCursor);
}

loadJobs();