const createProduct = (product)=>{
    const container = context.getCollection();
    container.createDocument(
      container.getSelfLink(),
      product,
      function (err, documentCreated) {
        if (err) throw new Error("Error" + err.message)
        response.setBody(documentCreated)
      }
    )
}