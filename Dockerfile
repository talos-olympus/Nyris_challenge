# Feel free to replace this with a more appropriate Dockerfile.

# FROM alpine
# RUN apk update
# CMD ["apk", "fetch", "coffee"]

FROM python:3.9
WORKDIR /opt/picture_converter
COPY requirements.txt .
RUN pip install -r ./requirements.txt
COPY ImageConverter.py .
ENTRYPOINT ["python"]
CMD ["ImageConverter.py"]




