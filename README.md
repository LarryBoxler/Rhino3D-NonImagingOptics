# Rhino3D-NonImagingOptics
Plugin for Rhino3D to create and ray trace non-imaging optical curves

Visual studio project for a Rhino3D plugin written in C#

The plugin is written as an implementation of the plane curves described in Chapter 21 of the book "Introduction to Non Imaging Optics" 2nd edition by Julio Chaves.  

The plugin adds commands to Rhino that allow for the construction of the curves based on the inputs described by the corresponding curve type given in the book.  The plugin also allows for the generation of rays, the assigning of properties to the curves (reflection, refraction, or absorbing), and the tracing of rays through the curve system for a visual confirmation of the performance of a particular shape.  

The intent for this plugin is to provide a quick sandbox for developing a non-imaging optical curve shape to produce a desired output that can be used for building surfaces of actual optical devices. 
