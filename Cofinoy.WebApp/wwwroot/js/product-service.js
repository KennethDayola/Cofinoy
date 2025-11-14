export const ProductsService = {
    async getAllProducts() {
        try {
            const response = await fetch("/Menu/GetAllProducts", {
                method: "GET",
                headers: { "Accept": "application/json" }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            const result = await response.json();
            if (!result.success) {
                throw new Error(result.error || "Unknown error");
            }

            const products = (result.data || []).map(p => ({
                id: p.id,
                name: p.name || "Unnamed Product",
                description: p.description || "",
                price: typeof p.price === "number" ? p.price : 0,
                imageUrl: p.imageUrl || "/images/thumbnail.png",
                category: p.category || "Uncategorized",
                customizations: p.customizations || p.Customizations || [],
                isActive: typeof p.isActive === "boolean" ? p.isActive : true,
                status: p.status || "Available",
                stock: p.stock || "0",
                createdAt: p.createdAt
            }));

            return { success: true, data: products };
        } catch (error) {
            console.error("Error fetching products:", error);
            return { success: false, error: error.message };
        }
    },

    async getAllCustomizations() {
        try {
            const response = await fetch("/Menu/GetAllCustomizations", {
                method: "GET",
                headers: { "Accept": "application/json" }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            const result = await response.json();
            if (!result.success) {
                throw new Error(result.error || "Unknown error");
            }

            return { success: true, data: result.data || [] };
        } catch (error) {
            console.error("Error fetching customizations:", error);
            return { success: false, error: error.message };
        }
    },

    async getProductsByCategory(categoryName) {
        try {
            console.log("Fetching products for category:", categoryName);
            const response = await fetch(`/Menu/GetProductsByCategory?categoryName=${encodeURIComponent(categoryName)}`, {
                method: "GET",
                headers: { "Accept": "application/json" }
            });

            console.log("Response status:", response.status);
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            const result = await response.json();
            console.log("Raw API result:", result);
            if (!result.success) {
                throw new Error(result.error || "Unknown error");
            }

            const products = (result.data || []).map(p => ({
                id: p.id,
                name: p.name || "Unnamed Product",
                description: p.description || "",
                price: typeof p.price === "number" ? p.price : 0,
                imageUrl: p.imageUrl || "/images/thumbnail.png",
                category: p.category || "Uncategorized",
                customizations: p.customizations || p.Customizations || [],
                isActive: typeof p.isActive === "boolean" ? p.isActive : true,
                status: p.status || "Available",
                stock: p.stock || "0",
                createdAt: p.createdAt
            }));

            console.log("Mapped products:", products);
            return { success: true, data: products };
        } catch (error) {
            console.error("Error fetching products by category:", error);
            return { success: false, error: error.message };
        }
    },

    async getAllCategories() {
        try {
            console.log("Fetching all categories");
            const response = await fetch("/Menu/GetAllCategories", {
                method: "GET",
                headers: { "Accept": "application/json" }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            const result = await response.json();
            console.log("Categories result:", result);
            return result;
        } catch (error) {
            console.error("Error fetching categories:", error);
            return { success: false, error: error.message };
        }
    }
};

// Expose globally for non-module or cross-file access with cache-busted script tags
if (typeof window !== 'undefined') {
    window.ProductsService = ProductsService;
}