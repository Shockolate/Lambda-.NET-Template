﻿UPDATE Product
SET Name = @Name,
Quantity = @Quantity,
Price = @Price
WHERE ProductCrn = @ProductCrn