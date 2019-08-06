//
//  ViewController.swift
//  CouchDraw
//
//  Created by Robert Hedgpeth on 8/5/19.
//  Copyright © 2019 Robert Hedgpeth. All rights reserved.
//

import UIKit

class ViewController: UIViewController {

    var lastPoint = CGPoint.zero
    var color = UIColor.black
    var brushWidth: CGFloat = 6.0
    var opacity: CGFloat = 1.0
    var swiped = false
    
    @IBOutlet weak var tempImageView: UIImageView!
    @IBOutlet weak var mainImageView: UIImageView!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        // Do any additional setup after loading the view.
    }
    
    @IBAction func colorButton1_Tapped(_ sender: Any) {
        color = UIColor.black
    }
    
    @IBAction func colorButton2_Tapped(_ sender: Any) {
        color = UIColor.red
    }
    
    @IBAction func colorButton3_Tapped(_ sender: Any) {
        color = UIColor.blue
    }
    
    @IBAction func colorButton4_Tapped(_ sender: Any) {
        color = UIColor.green
    }
    
    @IBAction func undo_Tapped(_ sender: Any) {
        
    }
    
    @IBAction func clearAll_Tapped(_ sender: Any) {
        
    }
    
    override func touchesBegan(_ touches: Set<UITouch>, with event: UIEvent?) {
        guard let touch = touches.first else {
            return
        }
        swiped = false
        lastPoint = touch.location(in: view)
    }
    
    func drawLine(from fromPoint: CGPoint, to toPoint: CGPoint) {
        // 1
        UIGraphicsBeginImageContext(view.frame.size)
        
        guard let context = UIGraphicsGetCurrentContext() else {
            return
        }
        
        tempImageView.image?.draw(in: view.bounds)
        
        // 2
        context.move(to: fromPoint)
        context.addLine(to: toPoint)
        
        // 3
        context.setLineCap(.round)
        context.setBlendMode(.normal)
        context.setLineWidth(brushWidth)
        context.setStrokeColor(color.cgColor)
        
        // 4
        context.strokePath()
        
        // 5
        tempImageView.image = UIGraphicsGetImageFromCurrentImageContext()
        tempImageView.alpha = opacity
        
        UIGraphicsEndImageContext()
    }
    
    override func touchesMoved(_ touches: Set<UITouch>, with event: UIEvent?) {
        guard let touch = touches.first else {
            return
        }
        
        // 6
        swiped = true
        let currentPoint = touch.location(in: view)
        drawLine(from: lastPoint, to: currentPoint)
        
        // 7
        lastPoint = currentPoint
    }
    
    override func touchesEnded(_ touches: Set<UITouch>, with event: UIEvent?) {
        if !swiped {
            // draw a single point
            drawLine(from: lastPoint, to: lastPoint)
        }
        
        // Merge tempImageView into mainImageView
        UIGraphicsBeginImageContext(mainImageView.frame.size)
        mainImageView.image?.draw(in: view.bounds, blendMode: .normal, alpha: 1.0)
        tempImageView?.image?.draw(in: view.bounds, blendMode: .normal, alpha: opacity)
        mainImageView.image = UIGraphicsGetImageFromCurrentImageContext()
        UIGraphicsEndImageContext()
        
        tempImageView.image = nil
    }
}

