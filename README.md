# Image Conversion Service

The goal of this project is to implement a Web API that converts
common web image file formats (e.g., PNG) into JPEG.

## Implementation notes

- The current server project is just a template project with OpenTelemetry tracing added on top.
- A `POST` request is made to the `/convert` route.
- The response is either a binary stream with `Content-Type: image/jpeg` or error information.
- The service should be packaged into a [`Dockerfile`](Dockerfile)
  and should be added to  [`docker-compose.yml`](docker-compose.yml)
  with appropriate configuration; publishing the image (e.g. Docker Hub) is not required.
- Feel free to use any existing solution as long as the above holds.

## Assumptions

You can make the following assumptions about the environment:

- The service is orchestrated by Kubernetes, which is configured to auto-scale it based on
  CPU or memory load.
- The service is called once for each search request; search requests must finish in
  under a second total (i.e., including the actual search).
- The service called once on each imported reference image; this is an off-line process and
  may take much longer.
- Search images are typically already JPEG at about 60 KB of compressed
  file size, but can be any typical web image format.
- Search images typically have around 1024 x 1024 pixels in total.
- Import images may be much larger.
- The `Content-Type` header is generally not trustworthy.

## Evaluation

As far as your code is concerned:

- Please follow common guidelines and professional practices and use a reasonable coding style.
- If you deviate from common sense or simplify on purpose, please add a note.

Feel free to replace this `README.md` with your own content that documents your service, but
feel also free not to do it.

For evaluation, the service will be started using

```console
$ docker-compose up --build
```

Apart from your service, this will start

- a Jaeger UI [localhost:16686](http://localhost:16686/) that you can use to trace individual request
- a httpbin on [localhost:8000](http://localhost:8000/) that can be used to test / echo requests.

Please do not remove any of those services.

With Docker Compose up and running, the application in the [tools/Imagination](tools/Imagination)
directory will be used to send requests against your service by starting

```console
$ dotnet run --project tools/Imagination
```

For testing, you can point it to a different directory using the `--TestFiles:BaseDirectory` command-line argument.

The responses from your service are not automatically evaluated by the tool. You _may choose_ to
add extra logic to it, that is - however - not required. If you do, please add a note.
