# ProductsCRUD API
### ProductsCRUD was an API I developed to train my C# .NET WEB skills. <br>
### It has three HTTP requests to create, view, and delete products. One of its distinguishing features is that it supports images, receiving base64 via POST and returning a public image link for the frontend to use via GET.

# Technologies used

### C# .NET for WEB <br>
### Neon for database <br>
### Cloudinary for image storage <br>
### Shard Cloud for backend deployment

# HTTP requests

## /getproducts
how to use:
```js
fetch('https://productapi.shardweb.app/getproducts')
  .then(response => {
    if (!response.ok) {
      throw new Error(`Error in request: ${response.status}`);
    }
    return response.json(); // Convert the response to JSON
  })
  .then(data => {
    console.log(data); // Do something with the data
  })
```

## /postproducts
### Note: the imgUrl must be in Base64
How to use:
```js
fetch('https://productapi.shardweb.app/postproducts', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    name: "Pizza",
    description: "Pizza Napolitana",
    price: "20",
    imgUrl: "base64 da img"
  })
})
```

## /deleteproducts
How to use:
```js
fetch('https://productapi.shardweb.app/deleteproducts?id=idaqui', {
  method: 'DELETE', 
  headers: { 
    'Content-Type': 'application/json' 
  }
});
```

## /updateproducts
### Note: you need to fill all the Fields 
How to use:
```js
fetch(https://productapi.shardweb.app/updateproducts, {
  method: 'PATCH',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    id: 2
    name: "Pizza",
    description: "Pizza Napolitana",
    price: "20",
    imgUrl: "base64 da img"
  })
});
```

## /updatepartsofproducts
### Note: you just need to fill in the fields you need to move
How to use:
```js
fetch(https://productapi.shardweb.app/updatepartsofproducts, {
  method: 'PUT',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    id: 2 //MANDATORY
    name: "Pizza", //OPCIONAL
    description: "Pizza Napolitana", //OPCIONAL
    price: "20", //OPCIONAL
    imgUrl: "base64 da img" //OPCIONAL
  })
});
```
