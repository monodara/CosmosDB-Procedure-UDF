function createProduct(product){
    const context = getContext();
    const container = context.getCollection();
    const response = context.getResponse()
    container.createDocument(
      container.getSelfLink(),
      product,
      function (err, documentCreated) {
        if (err) throw new Error("Error" + err.message)
        response.setBody(documentCreated)
      }
    )
}