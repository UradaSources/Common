#include <cstdint>
#include <cstdio>
#include <stdint.h>

#define STBIW_WINDOWS_UTF8

#define STB_IMAGE_IMPLEMENTATION
#include "stb/stb_image.h"

#define STB_IMAGE_RESIZE_IMPLEMENTATION
#include "stb/stb_image_resize.h"

#define STB_IMAGE_WRITE_IMPLEMENTATION
#include "stb/stb_image_write.h"

struct ImgHandle
{
  public:
    int w;
    int h;
    int comp;
    std::uint8_t *pixels;
};

extern "C"
{
    ImgHandle LoadImage(const std::uint8_t *data, int len)
    {
        std::uint8_t *pixels = nullptr;
        int w, h, comp;
        pixels = stbi_load_from_memory(data, len, &w, &h, &comp, STBI_rgb_alpha);

        return ImgHandle{w, h, comp, pixels};
    }

    ImgHandle LoadImageFromFile(const char *path)
    {
        std::uint8_t *pixels;
        int w, h, comp;

        pixels = stbi_load(path, &w, &h, &comp, STBI_rgb_alpha);

        return ImgHandle{w, h, comp, pixels};
    }

    void FreeImage(ImgHandle img)
    {
        if (img.pixels != nullptr)
            stbi_image_free(img.pixels);
    }

    ImgHandle ResizeImage(ImgHandle img, int w, int h)
    {
        int size = img.w * img.h * img.comp;
        auto buffer = (std::uint8_t *)stbi__malloc(size * sizeof(std::uint8_t));

        int flag = stbir_resize_uint8(img.pixels, img.w, img.h, 0, buffer, w, h, 0, 4);
        if (flag == 0)
        {
            STBI_FREE(buffer); // 若resize失败, 则立即释放内存
            buffer = nullptr;
        }

        return ImgHandle{w, h, img.comp, buffer};
    }

    bool SaveImageToPngFile(const char *path, ImgHandle img)
    {
        if (img.pixels == nullptr)
            return false;

        int flag = stbi_write_png(path, img.w, img.h, STBI_rgb_alpha, img.pixels, 0);
        return flag != 0;
    }

    bool CreateThumbnail(const char *srcPath, const char *dstPath, int fixedWidth)
    {
        auto img = LoadImageFromFile(srcPath);
        if (img.pixels == nullptr)
            return false;

        float r = (float)img.w / img.h;
        int h = fixedWidth / r;

        auto timg = ResizeImage(img, fixedWidth, h);
        if (timg.pixels == nullptr)
        {
            FreeImage(img);
            return false;
        }

        bool saved = SaveImageToPngFile(dstPath, timg);

        FreeImage(img);
        FreeImage(timg);

        return saved;
    }
}

// int main2(int argc, const char *argv[])
// {
//     const char *path = "C:/Users/Admin/Desktop/stbImgTest.png";

//     int w, h;
//     int comp;

//     std::printf("start load file %s\n", path);

//     // 加载图片
//     auto pixels = stbi_load(path, &w, &h, &comp, STBI_rgb_alpha);
//     if (pixels == nullptr)
//     {
//         std::printf("file %s load faild\n", path);
//         return -1;
//     }

//     std::printf("file load end. {%d, %d}", w, h);

//     // 设置尺寸
//     float r = (float)w / h;

//     const int fixedWidth = 300;
//     int newHeight = fixedWidth / r;

//     std::printf("new size: %d, %d; r : %f\n", fixedWidth, newHeight, r);

//     int size = fixedWidth * newHeight * STBI_rgb_alpha;
//     std::uint8_t *newPixels = (std::uint8_t *)stbi__malloc(size * sizeof(std::uint8_t));

//     int flag = stbir_resize_uint8(pixels, w, h, 0, newPixels, fixedWidth, newHeight, 0, 4);
//     if (flag == 0)
//     {
//         std::printf("resize operation faild\n");
//         return -1;
//     }

//     flag =
//         stbi_write_png("C:/Users/Admin/Desktop/stbImgTest2.png", fixedWidth, newHeight, STBI_rgb_alpha,
//         newPixels, 0);
//     if (flag == 0)
//     {
//         std::printf("write faild\n");
//         return -1;
//     }

//     std::printf("done");

//     stbi_image_free(pixels);
//     stbi_image_free(newPixels);

//     return 0;
// }