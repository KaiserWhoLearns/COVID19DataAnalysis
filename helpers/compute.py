import math


def roundup(num, nearest_ten=True):
    if nearest_ten:
        return int(math.ceil(num / 10.0)) * 10
    return int(math.ceil(num))
