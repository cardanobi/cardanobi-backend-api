# CardanoBI API Backend

## Getting Started

Our backend is written in .NET 6.0, it leverages [cardano-db-sync](https://github.com/input-output-hk/cardano-db-sync) and provides denormalized end-points to expose on-chain data into user friendly responses.

## Why denormalized?

Cardano-db-sync's data is normalized, because different aspects of data entities are stored in different tables (e.g. transactions, transaction inputs, transaction outputs).

This promotes data integrity and prevents redundancy, as each piece of data is stored in its most appropriate place.

However this is not ideal for workloads requiring read optimized data stores to achieve their business outcome, as a normalized data layer translates into fragmented data objects.

This is where denormalization comes into play: by taking data from multiple tables and presenting it as one comprehensive object, you're reducing the 'joins' a client would have to perform to obtain a full picture of a given data entity.

CardanoBI's denormalized approach improves read performance and make it easier for our clients get a full picture of the on-chain data they really care about to conduct their own business processes.

## Installation

### .Net 6.0

```sh
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get update
sudo apt-get install -y apt-transport-https
sudo apt-get update
sudo apt-get install -y dotnet-sdk-6.0

rm packages-microsoft-prod.deb
```

### CardanoBI API Backend
```
$ git clone https://github.com/cardanobi/cardanobi-backend-api.git
cd cardanobi-backend-api/src
dotnet build
dotnet run
```

## System Requirements

As the Cardano blockchain grows so do the storage requirements! The recommended specs presented below are as of 03/07/2023.

For the minimum deployment topology (e.g. 1 server):

- 64 GB of RAM
- 8 CPU cores
- 750 GB of disk storage
- SSD at least 50k IOPS

This will allow you to run ```cardano-node```, ```cardano-db-sync```, ```postgres```, ```cardanobi-backend-api``` all on the same server.

## Contributions

CardanoBI is fully open-source and everyone is welcome to contribute. Please reach out to us via twitter, email (info@cardanobi.io) or by submitting a PR. :heart: