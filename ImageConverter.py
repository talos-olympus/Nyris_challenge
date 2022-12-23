from flask import Flask, request, make_response
from PIL import Image
from io import BytesIO

ImageConverter = Flask(__name__)

@ImageConverter.route('/convert', methods=['POST'])
def convert_to_jpeg():
    try:
        # Get image data from request body
        image_data = request.data
        
        if not image_data:
            return make_response(str('No file recieved'))
        
        # Create a PIL image object with the image data, wrapping into a bytes file-like object
        # , and convert to RGB
        request_image = Image.open(BytesIO(image_data))
        temp_image = request_image.convert('RGB') # Deals with transparency
        
        # Save the request image data into a Bytes file-like object (jpeg_data), 
        # using JPEG format
        jpeg_data = BytesIO()
        temp_image.save(jpeg_data, format='JPEG')
        
        # Pull the saved jpeg data and create a response from it
        response = make_response(jpeg_data.getvalue())
        
        # Set header
        response.headers['Content-Type'] = 'image/jpeg'
    except Exception as e: # Handle exception
        response = make_response(str(e), 402)
        response.headers['Content-Type'] = 'text/plain'
        return response
    return response
 
# if __name__ == "__main__":
ImageConverter.run(debug=True, host='0.0.0.0', port='5000')